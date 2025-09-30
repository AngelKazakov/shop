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

    public async Task AddToCart(string userId, int productItemId, int quantity = 1)
    {
        ShoppingCart cart = await GetOrCreateCartAsync(userId);

        ShoppingCartItem? cartItem = cart.Items.FirstOrDefault(x => x.ProductItemId == productItemId);

        if (cartItem != null)
        {
            cartItem.Quantity += quantity;
        }
        else
        {
            cartItem = new ShoppingCartItem()
            {
                ShoppingCartId = cart.Id,
                ProductItemId = productItemId,
                Quantity = quantity,
            };

            cart.Items.Add(cartItem);
        }

        await this.context.SaveChangesAsync();
    }

    public async Task<ICollection<ShoppingCartItem>> GetCartItemsAsync(string userId)
    {
        ShoppingCart cart = await GetOrCreateCartAsync(userId);

        return cart.Items.ToList();
    }

    public async Task UpdateQuantity(string userId, int productItemId, int quantity)
    {
        ShoppingCart cart = await GetOrCreateCartAsync(userId);

        ShoppingCartItem? cartItem = cart.Items.FirstOrDefault(x => x.ProductItemId == productItemId);

        if (cartItem != null)
        {
            if (quantity <= 0)
            {
                context.ShoppingCartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
            }

            await context.SaveChangesAsync();
        }
    }

    public async Task RemoveFromCart(string userId, int productItemId)
    {
        ShoppingCart cart = await GetOrCreateCartAsync(userId);
        ShoppingCartItem? cartItem = cart.Items.FirstOrDefault(x => x.ProductItemId == productItemId);

        if (cartItem != null)
        {
            cart.Items.Remove(cartItem);
            await context.SaveChangesAsync();
        }
    }
}