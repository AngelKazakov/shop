using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Exceptions;
using RandomShop.Models.Product;
using System.Collections.ObjectModel;

namespace RandomShop.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly ShopContext context;
        private readonly IMapper mapper;

        public ProductService(IMapper mapper, ShopContext context)
        {
            this.mapper = mapper;
            this.context = context;
        }

        public async Task<Product> GetProductById(int productId)
        {
            Product product = await CheckIfProductExistsOrIsNull(productId);

            return product ?? throw new NotFoundException("Product not found.");
        }

        public async Task<Product> GetProductByName(string productName)
        {
            Product? product = await this.context.Products.Where(x => x.Name.Contains(productName)).FirstOrDefaultAsync();

            return product;
        }

        public async Task<ICollection<Product>> GetProductsByName(string productName)
        {
            string lowerProductName = $"%{productName.ToLower()}%";

            List<Product> products = await this.context.Products
                .Where(x => EF.Functions.Like(x.Name.ToLower(), lowerProductName))
                .Include(p => p.ProductItems)
                .ToListAsync();

            return products;
        }

        public async Task<ICollection<Product>> GetAllProducts()
        {
            try
            {
                List<Product>? prodcuts = await this.context.Products
              .AsNoTracking()
              .Include(p => p.ProductItems)
              .Include(p => p.ProductImages)
              .Include(p => p.ProductPromotions)
              .Include(p => p.ProductCategories)
              .ThenInclude(x => x.Category)
              .ToListAsync();

                return prodcuts;
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

        private async Task<Product> CheckIfProductExistsOrIsNull(int productId)
        {
            Product product = await this.context.Products.FindAsync(productId);

            return product;
        }
    }
}
