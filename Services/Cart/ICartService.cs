using RandomShop.Data.Models;

namespace RandomShop.Services.Cart;

public interface ICartService
{
    public Task<ShoppingCart> GetOrCreateCartAsync(string userId);
}