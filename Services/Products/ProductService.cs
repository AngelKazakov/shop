using System.Collections;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Exceptions;
using RandomShop.Models.Product;
using RandomShop.Services.Categories;
using System.Linq.Dynamic.Core;
using System.Reflection;
using NuGet.Packaging;
using RandomShop.Models.Variation;
using RandomShop.Services.Variation;
using RandomShop.Services.Promotions;
using RandomShop.Services.Images;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using RandomShop.Services.User;
using System.Threading.Tasks;
using System.Security.Claims;


namespace RandomShop.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly ShopContext context;
        private readonly IMapper mapper;
        private readonly ICategoryService categoryService;
        private readonly IImageService imageService;
        private readonly IVariationService variationService;
        private readonly IPromotionService promotionService;
        private readonly IHttpContextAccessor httpContextAccessor;


        public ProductService(IMapper mapper, ShopContext context, ICategoryService categoryService,
            IVariationService variationService, IPromotionService promotionService, IImageService imageService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.mapper = mapper;
            this.context = context;
            this.categoryService = categoryService;
            this.variationService = variationService;
            this.promotionService = promotionService;
            this.imageService = imageService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProductAddFormModel> InitProductAddFormModel(int categoryId)
        {
            return new ProductAddFormModel()
            {
                Categories = await this.categoryService.GetAllCategories(),
                VariationOptions = await this.variationService.GetVariationOptionBySpecifyCategory(categoryId),
            };
        }

        //private string? GetCurrentUserId()
        //{
        //    return httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //}

        public async Task<int> AddProduct(ProductAddFormModel model)
        {
            //Make nullable PromotionId in ProductAddFormModel and Database models also.
            Product product = await CreateProduct(model);
            ProductItem productItem = await CreateProductItem(model, product);

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await AddProductPromotion(product.Id, model.PromotionId);
                await AddProductConfigurations(model, productItem.Id);
                await CreateProductCategory(product.Id, model.CategoryId);

                ICollection<ProductImage> productImages = imageService.CreateProductImages(model.Images, product.Id);
                AddProductImagesToProduct(productImages, product);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                await imageService.SaveImages(model.Images, productImages);

                return productItem.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        //Check if current user is able to edit the product?.
        public async Task<int> EditProduct(ProductEditFormModel model)
        {
            ProductItem? modelForedit = await GetProductItemQuery()
                                            .FirstOrDefaultAsync(x => x.Id == model.Id) ??
                                        throw new KeyNotFoundException($"Product with ID {model.Id} not found.");

            try
            {
                // Update basic product details (async, since we fetch the discount rate)
                await UpdateProductDetails(modelForedit, model);

                //Handle images (new and deletion)
                HandleImages(model, modelForedit);

                //Update category
                await UpdateProductCategory(modelForedit.Product, model.CategoryId);

                //Update promotion
                await UpdateProductPromotion(modelForedit.Product, model.PromotionId);

                //Update variations and options
                await UpdateVariationsAndOptions(model.SelectedVariationOptions, modelForedit.Id);

                //Save changes
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //Handle exception (log or rethrow with context)
                throw new InvalidOperationException("Error while editing product with ID {model.Id}", ex);
            }

            return modelForedit.Id;
        }

        private async Task UpdateVariationsAndOptions(Dictionary<int, int?> variations, int productItemId)
        {
            try
            {
                // Filter out null values (ensures only selected variation options are used)
                //Key == VariationId and Value == VariationOptionId
                Dictionary<int, int?> filteredVariations = variations
                    .Where(v => v.Value != null)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                // Extract all non-null VariationOption Id's values- these are the selected options
                List<int> variationOptionIds = filteredVariations.Values
                    .Where(v => v.HasValue)
                    .Select(v => v.Value)
                    .ToList();

                // Query database for matching ProductConfigurations- we only need to update existing ones
                List<ProductConfiguration> existingProductConfigurations = await this.context.ProductConfigurations
                    .Where(pc => pc.ProductItemId == productItemId).ToListAsync();

                await RemoveVariationsWhichAreNoNeeded(existingProductConfigurations, variationOptionIds);
                await AddNewProductConfigurations(variationOptionIds, existingProductConfigurations, productItemId);

                await this.context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error (assuming you have a logging system)
                Console.WriteLine($"Error while updating variations for ProductItemId {productItemId}: {ex.Message}");

                // Optionally, rethrow the exception if you want the calling method to handle it
                throw;
            }
        }

        public async Task<ProductViewModel> GetProductById(int productId, string? userId = null)
        {
            ProductItem? productItem = await
                GetProductItemQuery()
                    .AsNoTracking()
                    .Where(x => x.Id == productId)
                    .FirstOrDefaultAsync();

            if (productItem == null)
            {
                throw new NotFoundException("Product not found.");
            }

            ProductViewModel productViewModel = await CreateProductViewModel(productItem, userId);
            return productViewModel;
        }

        public async Task<ProductEditFormModel> InitProductEditFormModel(int productId)
        {
            ProductDetailsDto? productDetails = await
                this.context.ProductItems.AsNoTracking()
                    .Where(x => x.Id == productId)
                    .Select(x => new ProductDetailsDto
                    {
                        Price = x.Price,
                        Name = x.Product.Name,
                        Description = x.Product.Description,
                        QuantityInStock = x.QuantityInStock,
                        SKU = x.SKU,
                        CategoryId = x.Product.ProductCategories.Select(pc => pc.CategoryId).FirstOrDefault(),
                        PromotionId = x.Product.ProductPromotions.Select(pp => pp.PromotionId).FirstOrDefault(),
                        ExistingVariationOptions = x.ProductConfigurations.Select(pc => new VariationViewModel
                        {
                            Name = pc.VariationOption.Variation.Name,
                            Value = pc.VariationOption.Value
                        }).ToList(),
                    })
                    .FirstOrDefaultAsync();

            if (productDetails == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }

            ProductEditFormModel? model = await CreateProductEditFormModel(productId, productDetails);

            return model;
        }

        private async Task<ProductEditFormModel> CreateProductEditFormModel(int productId,
            ProductDetailsDto productDetails, bool isAdmin = false)
        {
            ProductEditFormModel model = new ProductEditFormModel
            {
                Id = productId,
                Price = productDetails.Price,
                Name = productDetails.Name,
                Description = productDetails.Description,
                SKU = productDetails.SKU,
                QuantityInStock = productDetails.QuantityInStock,
                Categories = await this.categoryService.GetAllCategories(),
                CategoryId = productDetails.CategoryId ??
                             throw new InvalidOperationException("Category ID cannot be null."),
                // Images = await imageService.ReadImagesAsByteArrayAsync(productId),
                ExistingImages = await imageService.CreateProductImageViewModelAsync(productId),
                PromotionId = productDetails.PromotionId ?? 0,
                Promotions = await this.promotionService.GetAllPromotions(),
                ExistingVariationOptions = CreateVariationsDictionary(productDetails.ExistingVariationOptions),
                AllVariationOptions = await this.variationService.GetVariationsAndOptions(),
            };

            return model;
        }

        public async Task<ICollection<ProductListViewModel>> GetProductsByName(string productName)
        {
            string lowerProductName = $"%{productName.ToLower()}%";

            try
            {
                List<ProductListViewModel>? filteredProductsByName = await this.context.ProductItems.Where(x =>
                        x.QuantityInStock > 0 &&
                        EF.Functions.Like(x.Product.Name.ToLower(), $"%{lowerProductName}%"))
                    .Select(x => new ProductListViewModel()
                    {
                        Id = x.ProductId,
                        Name = x.Product.Name,
                        Price = x.DiscountedPrice > 0 ? x.DiscountedPrice : x.Price,
                        IsFavorite = false,
                    })
                    .ToListAsync();

                return filteredProductsByName;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching products by name.", ex);
            }
        }

        public async Task<ICollection<ProductListViewModel>> GetLatestAddedProducts()
        {
            try
            {
                List<ProductListViewModel>? latestProducts = await this.context.ProductItems
                    .Where(x => x.QuantityInStock > 0)
                    .OrderByDescending(x => x.CreatedOnDate)
                    .Take(3)
                    .Select(x => new ProductListViewModel()
                    {
                        Id = x.ProductId,
                        Name = x.Product.Name,
                        Price = x.DiscountedPrice > 0 ? x.DiscountedPrice : x.Price,
                        IsFavorite = false,
                    })
                    .ToListAsync();

                return latestProducts;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while getting latest products.", ex);
            }
        }

        public async Task<ICollection<ProductListViewModel>> GetAllProducts(string? userId = null)
        {
            //Apply the promotion after fetching products from database to simplify LINQ query.
            //Separate applying promotion from creating ProductViewModel and ProductListViewModel.
            try
            {
                List<ProductListViewModel>? products = await GetProductListProjection(userId).ToListAsync();

                return products;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching all products.", ex);
            }
        }

        private IQueryable<ProductListViewModel> GetProductListProjection(string? userId = null)
        {
            IQueryable<ProductListViewModel>? productItems = this.context.ProductItems.AsNoTracking()
                .Where(x => x.QuantityInStock > 0)
                .Select(x => new ProductListViewModel()
                {
                    Id = x.Product.Id,
                    Name = x.Product.Name,
                    Price = x.DiscountedPrice > 0 ? x.DiscountedPrice : x.Price,
                    IsFavorite = userId != null &&
                                 x.Product.UserFavoriteProducts.Any(fav =>
                                     fav.UserId == userId && fav.ProductId == x.Product.Id),
                });

            return productItems;
        }

        public async Task<ICollection<ProductListViewModel>> GetProductsByCategory(int categoryId)
        {
            try
            {
                List<ProductListViewModel>? products = await GetProductListProjection()
                    .Where(p => this.context.ProductCategories.Any(pc =>
                        pc.CategoryId == categoryId && pc.ProductId == p.Id)).ToListAsync();

                return products;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching products by category.", ex);
            }
        }

        public async Task<ICollection<ProductListViewModel>> GetProductsByPromotion(int promotionId)
        {
            try
            {
                List<ProductListViewModel>? products = await GetProductListProjection()
                    .Where(p => this.context.ProductPromotions.Any(pp =>
                        pp.PromotionId == promotionId && pp.ProductId == p.Id)).ToListAsync();

                return products;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching products by promotion.", ex);
            }
        }

        public async Task<ICollection<Product>> GetProductsByPriceRange(int minPrice, int maxPrice)
        {
            List<Product>? products = await this.context.ProductItems
                .AsNoTracking()
                .Where(x => x.Price >= minPrice && x.Price <= maxPrice)
                .Include(x => x.Product)
                .ThenInclude(x => x.ProductItems)
                .ThenInclude(x => x.ProductItemImages)
                .Select(x => x.Product)
                .ToListAsync();

            return products;
        }

        public async Task<ProductViewModel> UpdateStock(int productId, int quantity)
        {
            ProductItem? productItem = await this.context.ProductItems.FindAsync(productId);

            if (productItem == null)
            {
                throw new NotFoundException("Product not found.");
            }

            productItem.QuantityInStock = quantity;
            await this.context.SaveChangesAsync();


            return this.mapper.Map<ProductViewModel>(productItem);
        }

        public async Task<ProductViewModel> UpdatePrice(int productId, decimal price)
        {
            ProductItem? productItem = await this.context.ProductItems.FindAsync(productId);

            if (productItem == null)
            {
                throw new NotFoundException("Product not found.");
            }

            productItem.Price = price;
            await this.context.SaveChangesAsync();


            return this.mapper.Map<ProductViewModel>(productItem);
        }

        public async Task<ICollection<ProductListViewModel>> SortProducts(string criteria)
        {
            if (string.IsNullOrWhiteSpace(criteria))
            {
                throw new NotFoundException("Criteria not found.");
            }

            var productItemProperties = typeof(ProductItem)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name)
                .ToList();

            // Validate criteria
            if (!productItemProperties.Contains(criteria, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid sorting criteria: {criteria}");
            }

            List<ProductListViewModel>? sortedProducts = await this.context.ProductItems
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.ProductItemImages)
                .OrderBy(criteria)
                .Select(x => new ProductListViewModel
                {
                    // Add properties as needed
                })
                .ToListAsync();

            return sortedProducts;
        }

        public async Task<bool> DeleteProduct(int productId)
        {
            Product product = await CheckIfProductExistsOrIsNull(productId);

            if (product is null)
            {
                return false;
            }

            //string tempImagePath = ImageMapper.MoveImagesToTempDirectory(productId);
            string tempImagePath = await imageService.MoveImagesToTempDirectoryAsync(productId);

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                context.Products.Remove(product);

                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                //ImageMapper.PermanentlyDeleteTempImageDirectory(tempImagePath);
                await imageService.PermanentlyDeleteTempImageDirectoryAsync(tempImagePath);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                //ImageMapper.RestoreImagesFromTempDirectory(tempImagePath, productId);
                await imageService.RestoreImagesFromTempDirectoryAsync(tempImagePath, productId);
                throw new ApplicationException("An error occurred while deleting the product.", ex);
            }
        }

        public async Task<bool> DeleteSelectedProducts(List<int> productIds)
        {
            List<Product> products = await this.context.Products.Where(x => productIds.Contains(x.Id)).ToListAsync();

            if (!products.Any())
            {
                return false;
            }

            this.context.Products.RemoveRange(products);

            foreach (var product in products)
            {
                DeleteImageDirectoryByProductId(product.Id);
            }

            await this.context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkDeleteProducts(List<int> productIds)
        {
            await using (var transaction = await this.context.Database.BeginTransactionAsync())
            {
                try
                {
                    // await this.context.Products.Where(x => productIds.Contains(x.Id)).ExecuteDeleteAsync();

                    List<Product>? productsForDeletion = await this.context.Products
                        .Where(x => productIds.Contains(x.Id))
                        .ToListAsync();


                    if (productsForDeletion.Count() != productIds.Count())
                    {
                        throw new NotFoundException("One or more products are not found.");
                    }

                    this.context.Products.RemoveRange(productsForDeletion);
                    await this.context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new ApplicationException("An error occurred while deleting products.", ex);
                }
            }
        }

        private async Task<ProductViewModel> CreateProductViewModel(ProductItem productItem, string? userId = null)
        {
            //Log the try-catch exceptions.
            ProductViewModel productViewModel = new ProductViewModel()
            {
                Id = productItem.Id,
                Name = productItem.Product.Name,
                Description = productItem.Product.Description,
                SKU = productItem.SKU,
                //  Price = productItem?.DiscountedPrice ?? productItem.Price,
                Price = (productItem?.DiscountedPrice > 0) ? productItem.DiscountedPrice : productItem.Price,
                Category = productItem.Product.ProductCategories.Select(pc => pc.Category.Name).FirstOrDefault(),
                Promotion = productItem.Product.ProductPromotions.Select(pp => pp.Promotion.Name).FirstOrDefault(),
                Variations = productItem.ProductConfigurations.Select(pc => new VariationViewModel()
                {
                    Name = pc.VariationOption.Variation.Name,
                    Value = pc.VariationOption.Value
                }).ToList(),
                //Images = ImageMapper.ReadImagesAsByteArray(productItem.ProductId),
                Images = await imageService.ReadImagesAsByteArrayAsync(productItem.ProductId),
                IsFavorite = userId != null && productItem.Product.UserFavoriteProducts.Any(fav =>
                    fav.UserId == userId && fav.ProductId == productItem.ProductId),
            };

            productViewModel.VariationsAndOptions = CreateVariationsDictionary(productViewModel.Variations);
            return productViewModel;
        }

        private async Task UpdateProductDetails(ProductItem productItem, ProductEditFormModel model)
        {
            productItem.Product.Name = model.Name;
            productItem.Product.Description = model.Description;
            productItem.Price = model.Price;
            productItem.SKU = model.SKU;
            productItem.QuantityInStock = model.QuantityInStock;

            int? discountRate = await GetPromotionDiscountRateByIdAsync(model.PromotionId) ?? 0;

            productItem.DiscountedPrice = ApplyPromotionToProduct(model.Price, discountRate);
        }

        private async Task AddNewProductConfigurations(List<int> variationOptionIds,
            List<ProductConfiguration> existingProductConfigurations, int productItemId)
        {
            List<ProductConfiguration> newProductConfigurations = new List<ProductConfiguration>();

            foreach (var variationOptionId in variationOptionIds)
            {
                if (!existingProductConfigurations.Select(x => x.VariationOptionId).Contains(variationOptionId))
                {
                    newProductConfigurations.Add(new ProductConfiguration()
                    {
                        ProductItemId = productItemId,
                        VariationOptionId = variationOptionId
                    });
                }
            }

            await this.context.ProductConfigurations.AddRangeAsync(newProductConfigurations);
        }

        private Task RemoveVariationsWhichAreNoNeeded(List<ProductConfiguration> existingProductConfigurations,
            List<int> variationOptionIds)
        {
            var productConfigurationsForDeletion = new List<ProductConfiguration>();

            foreach (var productConfiguration in existingProductConfigurations)
            {
                if (!variationOptionIds.Contains(productConfiguration.VariationOptionId))
                {
                    productConfigurationsForDeletion.Add(productConfiguration);
                }
            }

            this.context.ProductConfigurations.RemoveRange(productConfigurationsForDeletion);

            return Task.CompletedTask;
        }

        private async Task UpdateProductCategory(Product product, int newCategoryId)
        {
            ProductCategory? existingProductCategory = product.ProductCategories.FirstOrDefault();

            if (existingProductCategory == null || existingProductCategory.CategoryId != newCategoryId)
            {
                if (existingProductCategory != null)
                {
                    context.ProductCategories.Remove(existingProductCategory);
                }

                product.ProductCategories.Add(new ProductCategory()
                    { ProductId = product.Id, CategoryId = newCategoryId });
            }
        }

        private async Task UpdateProductPromotion(Product product, int newPromotionId)
        {
            ProductPromotion? existingProductPromotion = product.ProductPromotions.FirstOrDefault();
            if (existingProductPromotion == null || existingProductPromotion.PromotionId != newPromotionId)
            {
                if (existingProductPromotion != null)
                {
                    context.ProductPromotions.Remove(existingProductPromotion);
                }

                product.ProductPromotions.Add(new ProductPromotion()
                    { ProductId = product.Id, PromotionId = newPromotionId });
            }
        }

        private List<int> ParseImagesForDeletion(string imagesForDelete)
        {
            if (string.IsNullOrWhiteSpace(imagesForDelete))
            {
                return new List<int>();
            }

            return imagesForDelete
                .Split(',')
                .Select(id => int.TryParse(id, out var result) ? (int?)result : null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();
        }

        private async void HandleImages(ProductEditFormModel model, ProductItem productItem)
        {
            List<int> imagesIdsForDeletion = ParseImagesForDeletion(model.ImagesForDelete);
            imageService.DeleteProductImages(imagesIdsForDeletion, productItem.Id);

            ICollection<ProductImage>
                newImages = imageService.CreateProductImages(model.NewAddedImages, productItem.Id);
            productItem.ProductItemImages.AddRange(newImages);
        }

        private Dictionary<string, List<string>> CreateVariationsDictionary(IEnumerable<VariationViewModel> variations)
        {
            return variations
                .GroupBy(v => v.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(v => v.Value).Distinct().ToList()
                );
        }

        private decimal CalculateDiscountedPrice(decimal price, int discountRate)
        {
            return price * (1 - (discountRate / (decimal)Data.DataConstants.PercentageDivisor));
        }


        private async Task<int?> GetPromotionDiscountRateByIdAsync(int promotionId)
        {
            return await this.context.Promotions
                .Where(p => p.Id == promotionId)
                .Select(p => (int?)p.DiscountRate)
                .FirstOrDefaultAsync();
        }

        private decimal ApplyPromotionToProduct(decimal currentPrice, int? discountRate)
        {
            if (discountRate.HasValue && discountRate > 0)
            {
                return CalculateDiscountedPrice(currentPrice, discountRate.Value);
            }

            return currentPrice;
        }

        private void DeleteImageDirectoryByProductId(int productId)
        {
            string productImagesPath = Path.Combine(DataConstants.ImagesPath, $"Product{productId}");

            if (Directory.Exists(productImagesPath))
            {
                Directory.Delete(productImagesPath, recursive: true);
            }
        }

        private void AddProductImagesToProduct(ICollection<ProductImage> productImages, Product product)
        {
            product.ProductImages.AddRange(productImages);
        }

        private async Task<Product> CheckIfProductExistsOrIsNull(int productId)
        {
            return await this.context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == productId);
        }

        private async Task<Product> CreateProduct(ProductAddFormModel model)
        {
            Product product = new Product()
            {
                Name = model.Name,
                Description = model.Description,
                CreatedOnDate = DateTime.Now,
            };

            await this.context.Products.AddAsync(product);

            return product;
        }

        private async Task<ProductItem> CreateProductItem(ProductAddFormModel model, Product product)
        {
            ProductItem productItem = new ProductItem()
            {
                Price = model.Price,
                SKU = model.SKU,
                QuantityInStock = model.QuantityInStock,
                Product = product,
                CreatedOnDate = DateTime.Now,
            };

            int? discountRate = await GetPromotionDiscountRateByIdAsync(model.PromotionId);

            productItem.DiscountedPrice = ApplyPromotionToProduct(model.Price, discountRate);

            await this.context.ProductItems.AddAsync(productItem);
            await this.context.SaveChangesAsync();

            return productItem;
        }

        private async Task AddProductConfigurations(ProductAddFormModel model, int productItemId)
        {
            List<ProductConfiguration> productConfigurationsList = model.SelectedVariationOptions.Select(
                variationOption => new ProductConfiguration
                {
                    ProductItemId = productItemId,
                    VariationOptionId = variationOption.VariationOptionId
                }).ToList();

            await context.ProductConfigurations.AddRangeAsync(productConfigurationsList);
        }

        private async Task AddProductPromotion(int productId, int promotionId)
        {
            if (promotionId != null && promotionId > 0)
            {
                ProductPromotion productPromotion = new ProductPromotion()
                {
                    ProductId = productId,
                    PromotionId = promotionId,
                };

                await this.context.ProductPromotions.AddAsync(productPromotion);
            }
        }

        private async Task CreateProductCategory(int productId, int categoryId)
        {
            await this.context.ProductCategories.AddAsync(new ProductCategory()
                { ProductId = productId, CategoryId = categoryId });
        }

        private IQueryable<ProductItem> GetProductItemQuery()
        {
            return this.context.ProductItems
                .Include(x => x.Product)
                .Include(x => x.Product.UserFavoriteProducts)
                .Include(x => x.Product.ProductCategories)
                .ThenInclude(x => x.Category)
                .Include(x => x.Product.ProductImages)
                .Include(x => x.ProductConfigurations)
                .ThenInclude(pc => pc.VariationOption)
                .ThenInclude(vo => vo.Variation)
                .Include(x => x.Product.ProductPromotions)
                .ThenInclude(p => p.Promotion);
        }
    }
}