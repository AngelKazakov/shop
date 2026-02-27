using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Models.Cart;

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
        ShoppingCart? cart = await GetCartWithItemsAsync(userId);

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

    public async Task<ICollection<CartItemViewModel>> GetCartItemsAsync(string userId)
    {
        ShoppingCart? cart = await context.ShoppingCarts
            .Include(c => c.Items)
            .ThenInclude(i => i.ProductItem)
            .ThenInclude(pi => pi.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
            return new List<CartItemViewModel>();

        return cart.Items.Select(ci => new CartItemViewModel
        {
            ProductItemId = ci.ProductItemId,
            ProductName = ci.ProductItem.Product.Name,
            Quantity = ci.Quantity,
            UnitPrice = ci.ProductItem.DiscountedPrice > 0 ? ci.ProductItem.DiscountedPrice : ci.ProductItem.Price,
        }).ToList();
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

    public async Task ClearCart(string userId)
    {
        ShoppingCart cart = await GetOrCreateCartAsync(userId);

        if (cart != null)
        {
            this.context.ShoppingCartItems.RemoveRange(cart.Items);
            context.ShoppingCarts.Remove(cart);
            await context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetCartTotal(string userId)
    {
        var cart = await context.ShoppingCarts
            .Include(c => c.Items)
            .ThenInclude(i => i.ProductItem)
            .ThenInclude(pi => pi.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        decimal totalAmount = cart.Items.Sum(i =>
            i.Quantity * (i.ProductItem.DiscountedPrice > 0 ? i.ProductItem.DiscountedPrice : i.ProductItem.Price));

        return totalAmount;
    }

    public async Task<int> GetCartTotalQuantity(string userId)
    {
        ShoppingCart? cart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
            return 0;

        return cart.Items.Sum(i => i.Quantity);
    }

    public async Task<bool> ValidateCart(string userId)
    {
        var cart = await context.ShoppingCarts
            .Include(c => c.Items)
            .ThenInclude(i => i.ProductItem)
            .ThenInclude(pi => pi.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        List<int> productItemIds = cart.Items.Select(i => i.ProductItemId).ToList();

        Dictionary<int, ProductItem> productItems = await context.ProductItems
            .Where(pi => productItemIds.Contains(pi.Id))
            .ToDictionaryAsync(pi => pi.ProductId, pi => pi);

        foreach (var item in cart.Items)
        {
            if (!productItems.ContainsKey(item.ProductItemId) ||
                productItems[item.ProductItemId].QuantityInStock < item.Quantity)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<(bool Success, string? Message, decimal ItemTotal, decimal GrandTotal)>
        UpdateQuantityAndGetTotals(string userId, int productItemId, int quantity)
    {
        await UpdateQuantity(userId, productItemId, quantity);

        ICollection<CartItemViewModel> cartItems = await GetCartItemsAsync(userId);

        CartItemViewModel? updatedItem = cartItems.FirstOrDefault(ci => ci.ProductItemId == productItemId);

        if (updatedItem == null)
        {
            return (false, "Item not found", 0, 0);
        }

        return (true, null, updatedItem.TotalPrice, cartItems.Sum(ci => ci.TotalPrice));
    }

    private async Task<ShoppingCart?> GetCartWithItemsAsync(string userId)
    {
        var cart = await context.ShoppingCarts
            .Include(c => c.Items)
            .ThenInclude(i => i.ProductItem)
            .ThenInclude(pi => pi.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        return cart;
    }
}
