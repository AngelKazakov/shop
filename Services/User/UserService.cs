using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;

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
            UserFavoriteProduct isAlreadyFavoriteProduct = await this.context.UserFavoriteProducts.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);

            if (isAlreadyFavoriteProduct == null)
            {
                return false;
            }

            return true;
        }


        public bool CheckIfProductIsAlreadyFavoriteSync(string userId, int productId)
        {
            UserFavoriteProduct isAlreadyFavoriteProduct = this.context.UserFavoriteProducts.FirstOrDefault(x => x.UserId == userId && x.ProductId == productId);

            if (isAlreadyFavoriteProduct == null)
            {
                return false;
            }

            return true;
        }
    }
}
