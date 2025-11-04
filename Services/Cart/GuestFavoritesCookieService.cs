using System.Text.Json;

namespace RandomShop.Services.Cart;

public class GuestFavoritesCookieService : IGuestFavoritesCookieService
{
    public ICollection<int> ReadGuestFavorites(HttpRequest request)
    {
        var cookie = request.Cookies["GuestFavorites"];

        if (string.IsNullOrEmpty(cookie))
        {
            return new List<int>();
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<int>>(cookie);

            return items ?? new List<int>();
        }
        catch (JsonException)
        {
            return new List<int>();
        }
    }
}