﻿using System.Collections;
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
using RandomShop.Models.Variation;
using RandomShop.Services.Promotions;
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
            //Make nullable PromotionId in ProductAddFormModel and Database models also.
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
            ProductItem? productItem = await this.context.ProductItems
                  .AsNoTracking()
                 .Include(x => x.Product)
                 .Include(x => x.ProductConfigurations)
                 .ThenInclude(pc => pc.VariationOption)
                 .ThenInclude(vo => vo.Variation)
                 .Include(x => x.Product.ProductPromotions)
                  .ThenInclude(p => p.Promotion)
                 .Where(x => x.Id == productId)
                 .FirstOrDefaultAsync();

            if (productItem == null)
            {
                throw new NotFoundException("Product not found.");
            }

            ProductViewModel productViewModel = CreateProductViewModel(productItem);
            ApplyPromotionToProduct(productViewModel);
            return productViewModel;
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
                var products = await this.context.ProductItems.AsNoTracking()
                      .Include(x => x.Product)
                      .Where(x => EF.Functions.Like(x.Product.Name.ToLower(), lowerProductName))
                      .Select(x => CreateProductListViewModel(x.Product, x))
                      .ToListAsync();

                ApplyPromotionToProduct(products);

                return products;
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
                var products = await this.context.ProductItems.AsNoTracking()
                     .Include(x => x.Product)
                     .Include(pp => pp.Product.ProductPromotions)
                     .ThenInclude(p => p.Promotion)
                     .OrderByDescending(x => x.CreatedOnDate)
                     .Take(3)
                     .Select(x => CreateProductListViewModel(x.Product, x))
                     .ToListAsync();

                ApplyPromotionToProduct(products);

                return products;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while getting latest products.", ex);
            }
        }

        public async Task<ICollection<ProductListViewModel>> GetAllProducts()
        {
            //Apply the promotion after fetching products from database to simplify LINQ query.
            //Separate applying promotion from creating ProductViewModel and ProductListViewModel.
            try
            {
                List<ProductListViewModel> products = await this.context.ProductItems.AsNoTracking()
                            .Include(x => x.Product)
                            .Include(pp => pp.Product.ProductPromotions)
                            .ThenInclude(p => p.Promotion)
                            .Select(x => CreateProductListViewModel(x.Product, x))
                            .ToListAsync();

                ApplyPromotionToProduct(products);

                return products;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while fetching all products.", ex);
            }
        }

        public async Task<ICollection<ProductListViewModel>> GetProductsByCategory(int categoryId)
        {
            try
            {
                List<ProductListViewModel> products = await this.context.ProductItems
                    .AsNoTracking()
                    .Include(x => x.Product)
                    .Include(pp => pp.Product.ProductPromotions)
                    .ThenInclude(p => p.Promotion)
                    .Where(p => p.Product.ProductCategories.Any(pc => pc.CategoryId == categoryId))
                    .Select(x => CreateProductListViewModel(x.Product, x))
                    .ToListAsync();

                ApplyPromotionToProduct(products);

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
                var products = await this.context.ProductItems
                     .AsNoTracking()
                     .Include(x => x.Product.ProductPromotions)
                     .ThenInclude(x => x.Promotion)
                     .Where(x => x.Product.ProductPromotions.Any(pp => pp.PromotionId == promotionId))
                     .Select(x => CreateProductListViewModel(x.Product, x))
                     .ToListAsync();

                ApplyPromotionToProduct(products);

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

        private ProductViewModel CreateProductViewModel(ProductItem productItem)
        {
            ProductViewModel productViewModel = new ProductViewModel()
            {
                Id = productItem.Id,
                Name = productItem.Product.Name,
                Description = productItem.Product.Description,
                SKU = productItem.SKU,
                Price = productItem.Price,
                Category = productItem.Product.ProductCategories.Select(pc => pc.Category.Name).FirstOrDefault(),
                Promotion = productItem.Product.ProductPromotions.Select(pp => pp.Promotion.Name).FirstOrDefault(),
                Discount = productItem.Product.ProductPromotions.Select(x => x.Promotion.DiscountRate).FirstOrDefault(),
                Variations = productItem.ProductConfigurations.Select(pc => new VariationViewModel()
                {
                    Name = pc.VariationOption.Variation.Name,
                    Value = pc.VariationOption.Value
                }).ToList(),
                Images = ImageMapper.ReadImagesAsByteArray(productItem.ProductId),
            };

            productViewModel.VariationsAndOptions = CreateVariationsDictionary(productViewModel.Variations);

            return productViewModel;
        }

        private static ProductListViewModel CreateProductListViewModel(Product product, ProductItem? productItem)
        {
            var productListViewModel = new ProductListViewModel()
            {
                Id = product.Id,
                Name = product.Name,
                Price = productItem?.Price ?? 0,
                Discount = productItem.Product.ProductPromotions.Select(x => x.Promotion.DiscountRate).FirstOrDefault(),
            };

            return productListViewModel;
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

        private static decimal CalculateDiscountedPrice(decimal price, int? discountRate)
        {
            return (decimal)(price * (1 - (discountRate / Data.DataConstants.PercentageDivisor)));
        }

        private static void ApplyPromotionToProduct(ProductListViewModel product)
        {
            if (product.Discount.HasValue && product.Discount > 0)
            {
                product.Price = CalculateDiscountedPrice(product.Price, product.Discount.Value);
            }
        }

        private static void ApplyPromotionToProduct(ProductViewModel product)
        {
            if (product.Discount > 0)
            {
                product.Price = CalculateDiscountedPrice(product.Price, product.Discount);
            }
        }

        private static void ApplyPromotionToProduct(IEnumerable<ProductListViewModel> products)
        {
            foreach (var product in products.Where(p => p.Discount.HasValue && p.Discount > 0))
            {
                ApplyPromotionToProduct(product);
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
