using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Exceptions;
using RandomShop.Models.Product;
using RandomShop.Services.Categories;
using System.Linq.Dynamic.Core;
using System.Reflection;
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

                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return productItem.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                throw;
            }
        }


        public async Task<Product> GetProductById(int productId)
        {
            Product product = await CheckIfProductExistsOrIsNull(productId);

            return product ?? throw new NotFoundException("Product not found.");
        }

        public async Task<Product> GetProductByName(string productName)
        {
            Product? product =
                await this.context.Products.Where(x => x.Name.Contains(productName)).FirstOrDefaultAsync();

            return product;
        }

        public async Task<ICollection<Product>> GetProductsByName(string productName)
        {
            string lowerProductName = $"%{productName.ToLower()}%";

            List<Product> products = await this.context.Products
                .AsNoTracking()
                .Where(x => EF.Functions.Like(x.Name.ToLower(), lowerProductName))
                .Include(p => p.ProductItems)
                .ToListAsync();

            return products;
        }

        public async Task<ICollection<Product>> GetAllProducts()
        {
            try
            {
                List<Product>? products = await this.context.Products
                    .AsNoTracking()
                    .Include(p => p.ProductItems)
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductPromotions)
                    .Include(p => p.ProductCategories)
                    .ThenInclude(x => x.Category)
                    .ToListAsync();

                return products;
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

            if (product == null)
            {
                throw new NotFoundException("Product not found.");
            }

            try
            {
                this.context.Products.Remove(product);
                await this.context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while deleting the product.", ex);
            }
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

    }
}
