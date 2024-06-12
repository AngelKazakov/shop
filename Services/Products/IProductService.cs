using RandomShop.Data.Models;

namespace RandomShop.Services.Products
{
    public interface IProductService
    {
        public Task<Product> GetProductById(int productId);

        public Task<bool> DeleteProduct(int productId);
    }
}
