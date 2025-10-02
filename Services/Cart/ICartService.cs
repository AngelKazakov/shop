using RandomShop.Data.Models;
using RandomShop.Models.Cart;

namespace RandomShop.Services.Cart;

public interface ICartService
{
    public Task<ShoppingCart> GetOrCreateCartAsync(string userId);

    public Task AddToCart(string userId, int productItemId, int quantity = 1);

    public Task<ICollection<CartItemViewModel>> GetCartItemsAsync(string userId);

    public Task UpdateQuantity(string userId, int productItemId, int quantity);

    public Task RemoveFromCart(string userId, int productItemId);

    public Task ClearCart(string userId);

    public Task<decimal> GetCartTotal(string userId);

    public Task<int> GetCartTotalQuantity(string userId);

    public Task<bool> ValidateCart(string userId);

    //Implement merge cart. Guest cart merge after user logged in.
}