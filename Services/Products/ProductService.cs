using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Models.Product;

namespace RandomShop.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly ShopContext context;

        public ProductService(ShopContext context)
        {
            this.context = context;
        }

        public async Task<Product> GetProductById(int productId)
        {
            Product product = await this.context.Products.FindAsync(productId);

            return product ?? throw new NullReferenceException("Product not found.");
        }
    }
}
