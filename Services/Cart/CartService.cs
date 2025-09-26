using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;

namespace RandomShop.Services.Cart;

public class CartService : ICartService
{
    private readonly ShopContext context;

    public CartService(ShopContext context)
    {
        this.context = context;
    }

    public async Task<ShoppingCart> GetOrCreateCartAsync(string userId)
    {
        ShoppingCart? cart = await context.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null)
        {
            ShoppingCart newCart = new ShoppingCart()
            {
                UserId = userId,
            };

            await this.context.ShoppingCarts.AddAsync(newCart);
            await this.context.SaveChangesAsync();

            return newCart;
        }

        return cart;
    }
}