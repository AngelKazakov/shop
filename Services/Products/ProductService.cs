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
using RandomShop.Infrastructure;
using RandomShop.Services.Variation;


namespace RandomShop.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly ShopContext context;
        private readonly IMapper mapper;
        private readonly ICategoryService categoryService;
        private readonly IVariationService variationService;

        public ProductService(IMapper mapper, ShopContext context, ICategoryService categoryService,
            IVariationService variationService)
        {
            this.mapper = mapper;
            this.context = context;
            this.categoryService = categoryService;
            this.variationService = variationService;
        }

        public async Task<ProductAddFormModel> InitProductAddFormModel(int categoryId)
        {
            return new ProductAddFormModel()
            {
                Categories = await this.categoryService.GetAllCategories(),
                VariationOptions = await this.variationService.GetVariationOptionBySpecifyCategory(categoryId),
            };
        }

        public async Task<int> AddProduct(ProductAddFormModel model)
        {
            Product product = await CreateProduct(model);
            ProductItem productItem = await CreateProductItem(model, product);

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await AddProductPromotion(product.Id, model.PromotionId);
                await AddProductConfigurations(model, productItem.Id);
                await CreateProductCategory(product.Id, model.CategoryId);

                ICollection<ProductImage> productImages = ImageMapper.CreateProductImages(model.Images, product.Id);
                AddProductImagesToProduct(productImages, product);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                await ImageMapper.SaveImages(model.Images, productImages);

                return productItem.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductViewModel> GetProductById(int productId)
        {
            var productItem = await this.context.ProductItems
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.ProductConfigurations)
                .ThenInclude(pc => pc.VariationOption)
                .ThenInclude(vo => vo.Variation)
                .Where(x => x.Id == productId)
                .Select(x => new
                {
                    x.Id,
                    ProductName = x.Product.Name,
                    ProductDescription = x.Product.Description,
                    x.Price,
                    x.SKU,
                    Category = x.Product.ProductCategories.Select(pc => pc.Category.Name).FirstOrDefault(),
                    Promotion = x.Product.ProductPromotions.Select(pp => pp.Promotion.Name).FirstOrDefault(),
                    Variations = x.ProductConfigurations.Select(pc => new
                    {
                        VariationName = pc.VariationOption.Variation.Name,
                        OptionValue = pc.VariationOption.Value
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (productItem == null)
            {
                throw new NotFoundException("Product not found.");
            }

            Dictionary<string, List<string>> variationsDictionary = productItem.Variations
                .GroupBy(v => v.VariationName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(v => v.OptionValue).Distinct().ToList()
                );

            return new ProductViewModel
            {
                Id = productItem.Id,
                Name = productItem.ProductName,
                Description = productItem.ProductDescription,
                Price = productItem.Price,
                SKU = productItem.SKU,
                Category = productItem.Category,
                Promotion = productItem.Promotion,
                VariationsAndOptions = variationsDictionary,
                Images = ImageMapper.ReadImagesAsByteArray(productId),
            };
        }

        public async Task<Product> GetProductByName(string productName)
        {
            Product? product =
                await this.context.Products.Where(x => x.Name.Contains(productName)).FirstOrDefaultAsync();

            return product;
        }

        public async Task<ICollection<ProductListViewModel>> GetProductsByName(string productName)
        {
            string lowerProductName = $"%{productName.ToLower()}%";

            try
            {
                return await this.context.ProductItems.AsNoTracking()
                    .Include(x => x.Product)
                    .Where(x => EF.Functions.Like(x.Product.Name.ToLower(), lowerProductName))
                    .Select(x => new ProductListViewModel()
                    {
                        Id = x.Id,
                        Name = x.Product.Name,
                        Price = x.Price
                    })
                    .ToListAsync();
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
                // Include promotion and set the promotion price if there is any discount.
                return await this.context.ProductItems.AsNoTracking()
                    .Include(x => x.Product)
                    .OrderByDescending(x => x.CreatedOnDate)
                    .Take(3)
                    .Select(x => new ProductListViewModel()
                    {
                        Id = x.Id,
                        Name = x.Product.Name,
                        Price = x.Price
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while getting latest products.", ex);
            }

        }

        public async Task<ICollection<ProductListViewModel>> GetAllProducts()
        {
            try
            {
                // Include promotion and set the promotion price if there is any discount.
                return await this.context.ProductItems.AsNoTracking()
                    .Include(x => x.Product)
                    .Select(x => new ProductListViewModel()
                    {
                        Id = x.Id,
                        Name = x.Product.Name,
                        Price = x.Price
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching all products.", ex);
            }
        }

        public async Task<ICollection<Product>> GetProductsByCategory(int categoryId)
        {
            List<Product>? products = await this.context.ProductCategories
                .AsNoTracking()
                .Where(x => x.CategoryId == categoryId)
                .Include(x => x.Product)
                .ThenInclude(x => x.ProductItems)
                .ThenInclude(x => x.ProductItemImages)
                .Include(x => x.Category)
                .Select(p => p.Product)
                .ToListAsync();

            return products;
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

            string tempImagePath = ImageMapper.MoveImagesToTempDirectory(productId);

            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                context.Products.Remove(product);

                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                ImageMapper.PermanentlyDeleteTempImageDirectory(tempImagePath);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                ImageMapper.RestoreImagesFromTempDirectory(tempImagePath, productId);
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
            ProductPromotion productPromotion = new ProductPromotion()
            {
                ProductId = productId,
                PromotionId = promotionId,
            };

            await this.context.ProductPromotions.AddAsync(productPromotion);
        }

        private async Task CreateProductCategory(int productId, int categoryId)
        {
            await this.context.ProductCategories.AddAsync(new ProductCategory()
            { ProductId = productId, CategoryId = categoryId });
        }


    }
}
