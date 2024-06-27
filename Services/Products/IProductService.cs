using RandomShop.Data.Models;

namespace RandomShop.Services.Products
{
    public interface IProductService
    {
        public Task<Product> GetProductById(int productId);

        public Task<bool> DeleteProduct(int productId);

        public Task<Product> GetProductByName(string productName);

        public Task<ICollection<Product>> GetProductsByName(string productName);

        public Task<ICollection<Product>> GetAllProducts();
    }
}
