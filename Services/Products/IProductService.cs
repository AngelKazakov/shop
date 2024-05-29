using RandomShop.Data.Models;

namespace RandomShop.Services.Products
{
    public interface IProductService
    {
        public Task<Product> GetProductById(int productId);
    }
}
