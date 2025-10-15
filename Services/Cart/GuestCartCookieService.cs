using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Models.Cart;
using RandomShop.Models.Cookie;

namespace RandomShop.Services.Cart;

public class GuestCartCookieService : IGuestCartCookieService
{
    private readonly ShopContext context;

    public GuestCartCookieService(ShopContext context)
    {
        this.context = context;
    }

    public void WriteGuestCart(HttpResponse response, List<CartCookieItem> items)
    {
        string json = JsonSerializer.Serialize(items);

        CookieOptions options = new CookieOptions()
        {
            Expires = DateTime.UtcNow.AddDays(7),
            HttpOnly = false,
            Secure = true,
        };

        response.Cookies.Append("GuestCart", json, options);
    }

    public List<CartCookieItem> ReadGuestCart(HttpRequest request)
    {
        string? cookie = request.Cookies["GuestCart"];

        if (string.IsNullOrEmpty(cookie))
        {
            return new List<CartCookieItem>();
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<CartCookieItem>>(cookie);

            return items ?? new List<CartCookieItem>();
        }
        catch (JsonException)
        {
            return new List<CartCookieItem>();
        }
    }

    public void AddOrUpdateGuestCart(HttpRequest request, HttpResponse response, int productItemId, int quantity)
    {
        List<CartCookieItem> items = ReadGuestCart(request);

        CartCookieItem? existingProduct = items.FirstOrDefault(x => x.ProductItemId == productItemId);

        if (existingProduct != null)
        {
            existingProduct.Quantity = Math.Clamp(existingProduct.Quantity + quantity, 1, 99);
        }
        else
        {
            int validQuantity = Math.Clamp(quantity, 1, 99);
            items.Add(new CartCookieItem { ProductItemId = productItemId, Quantity = validQuantity });
        }

        WriteGuestCart(response, items);
    }

    public async Task<CartViewModel> GetGuestCart(List<CartCookieItem>? guestItems)
    {
        if (guestItems == null || !guestItems.Any())
        {
            return new CartViewModel { Items = new List<CartItemViewModel>() };
        }

        List<int> productItemIds = guestItems.Select(x => x.ProductItemId).ToList();

        List<ProductItem> productItems = await this.context.ProductItems.Include(pi => pi.Product)
            .Where(pi => productItemIds.Contains(pi.Id))
            .ToListAsync();

        List<CartItemViewModel> productItemsViewModels = productItems.Select(x => new CartItemViewModel()
            {
                ProductItemId = x.Id,
                ProductName = x.Product.Name,
                UnitPrice = x.Price,
                Quantity = guestItems.FirstOrDefault(i => i.ProductItemId == x.Id)?.Quantity ?? 0,
            })
            .Where(vm => vm.Quantity > 0)
            .ToList();

        return new CartViewModel() { Items = productItemsViewModels };
    }

    public void UpdateGuestQuantity(HttpRequest request, HttpResponse response, int productItemId, int quantity)
    {
        List<CartCookieItem> items = ReadGuestCart(request);
        CartCookieItem? product = items.FirstOrDefault(x => x.ProductItemId == productItemId);

        if (product != null)
        {
            if (quantity <= 0)
            {
                items.Remove(product);
            }
            else
            {
                product.Quantity = quantity;
            }

            WriteGuestCart(response, items);
        }
    }
}