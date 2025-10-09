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
}