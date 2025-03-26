namespace RandomShop.Services.User
{
    public interface IUserService
    {
        public Task<bool> AddProductToFavorite(string userId, int productId);

        public Task<bool> CheckIfProductIsAlreadyFavorite(string userId, int productId);

        public bool CheckIfProductIsAlreadyFavoriteSync(string userId, int productId);
    }
}
