using RandomShop.Data.Models;

namespace RandomShop.Services.Cart;

public interface ICartService
{
    public Task<ShoppingCart> GetOrCreateCartAsync(string userId);

    public Task AddToCart(string userId, int productItemId, int quantity = 1);
    
    public Task<ICollection<ShoppingCartItem>> GetCartItemsAsync(string userId);
}