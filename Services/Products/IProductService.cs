﻿using RandomShop.Data.Models;
using RandomShop.Models.Product;

namespace RandomShop.Services.Products
{
    public interface IProductService
    {
        public Task<Product> GetProductById(int productId);

        public Task<bool> DeleteProduct(int productId);

        public Task<Product> GetProductByName(string productName);

        public Task<ICollection<Product>> GetProductsByName(string productName);

        public Task<ICollection<ProductListViewModel>> GetAllProducts();

        public Task<ICollection<Product>> GetProductsByCategory(int categoryId);

        public Task<ICollection<Product>> GetProductsByPriceRange(int minPrice, int maxPrice);

        public Task<ProductViewModel> UpdateStock(int productId, int quantity);

        public Task<ProductViewModel> UpdatePrice(int productId, decimal price);

        public Task<ICollection<ProductListViewModel>> SortProducts(string criteria);

        public Task<bool> BulkDeleteProducts(List<int> productIds);

        public Task<int> AddProduct(ProductAddFormModel model);

        public Task<ProductAddFormModel> InitProductAddFormModel(int categoryId);
    }
}
