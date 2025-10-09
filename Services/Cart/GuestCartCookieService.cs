using System.Text.Json;
using RandomShop.Models.Cookie;

namespace RandomShop.Services.Cart;

public class GuestCartCookieService : IGuestCartCookieService
{
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
}