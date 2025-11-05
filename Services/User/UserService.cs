using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Models.Product;

namespace RandomShop.Services.User
{
    public class UserService : IUserService
    {
        private readonly ShopContext context;

        public UserService(ShopContext context)
        {
            this.context = context;
        }

        public async Task<bool> AddProductToFavorite(string userId, int productId)
        {
            return await CreateUserFavoriteProduct(userId, productId);
        }

        public async Task<bool> RemoveProductFromFavorite(string userId, int productId)
        {
            UserFavoriteProduct? productForRemoval =
                await this.context.UserFavoriteProducts.FirstOrDefaultAsync(x =>
                    x.UserId == userId && x.ProductId == productId);

            if (productForRemoval != null)
            {
                this.context.UserFavoriteProducts.Remove(productForRemoval);
                await this.context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<ICollection<ProductListViewModel>> GetFavoriteProducts(string userId)
        {
            var favoriteProducts = await this.context.UserFavoriteProducts.Where(x => x.UserId == userId)
                .Select(x => new
                {
                    Product = x.Product,
                    FirstItem = x.Product.ProductItems.FirstOrDefault(),
                })
                .Select(x => new ProductListViewModel()
                {
                    Id = x.Product.Id,
                    Name = x.Product.Name,
                    Price = x.FirstItem.DiscountedPrice > 0 ? x.FirstItem.DiscountedPrice : x.FirstItem.Price,
                    IsFavorite = true,
                })
                .ToListAsync();

            return favoriteProducts;
        }

        public async Task<bool> ToggleFavoriteAsync(string userId, int productId)
        {
            bool isAlreadyFavorite = await CheckIfProductIsAlreadyFavorite(userId, productId);

            if (isAlreadyFavorite)
            {
                await RemoveProductFromFavorite(userId, productId);
                return false;
            }
            else
            {
                await AddProductToFavorite(userId, productId);
                return true;
            }
        }

        private async Task<bool> CreateUserFavoriteProduct(string userId, int productId)
        {
            if (!await CheckIfProductIsAlreadyFavorite(userId, productId))
            {
                try
                {
                    UserFavoriteProduct? newFavoriteProduct = new UserFavoriteProduct()
                    {
                        UserId = userId,
                        ProductId = productId,
                    };

                    await this.context.UserFavoriteProducts.AddAsync(newFavoriteProduct);
                    await this.context.SaveChangesAsync();

                    return true;
                }
                catch (Exception)
                {
                    //Catch the exception correctly.
                    throw;
                }
            }

            return false;
        }

        public async Task<bool> CheckIfProductIsAlreadyFavorite(string userId, int productId)
        {
            UserFavoriteProduct isAlreadyFavoriteProduct =
                await this.context.UserFavoriteProducts.FirstOrDefaultAsync(x =>
                    x.UserId == userId && x.ProductId == productId);

            if (isAlreadyFavoriteProduct == null)
            {
                return false;
            }

            return true;
        }


        public bool CheckIfProductIsAlreadyFavoriteSync(string userId, int productId)
        {
            UserFavoriteProduct isAlreadyFavoriteProduct =
                this.context.UserFavoriteProducts.FirstOrDefault(x => x.UserId == userId && x.ProductId == productId);

            if (isAlreadyFavoriteProduct == null)
            {
                return false;
            }

            return true;
        }
    }
}