using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;

namespace RandomShop.Services.Cart;

public class GuestFavoritesCookieService : IGuestFavoritesCookieService
{
    private readonly ShopContext context;

    public GuestFavoritesCookieService(ShopContext context)
    {
        this.context = context;
    }

    public ICollection<int> ReadGuestFavorites(HttpRequest request)
    {
        string? cookie = request.Cookies["GuestFavorites"];

        if (string.IsNullOrEmpty(cookie))
        {
            return new List<int>();
        }

        try
        {
            List<int>? items = JsonSerializer.Deserialize<List<int>>(cookie);

            return items ?? new List<int>();
        }
        catch (JsonException)
        {
            return new List<int>();
        }
    }

    public void WriteGuestFavorites(HttpResponse response, ICollection<int> productIds)
    {
        string json = JsonSerializer.Serialize(productIds);

        CookieOptions options = new CookieOptions()
        {
            Expires = DateTime.Now.AddDays(7),
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Lax,
        };

        response.Cookies.Append("GuestFavorites", json, options);
    }

    public void ClearGuestFavorites(HttpResponse response)
    {
        response.Cookies.Delete("GuestFavorites");
    }

    public bool ToggleFavorite(HttpRequest request, HttpResponse response, int productId)
    {
        ICollection<int> favorites = ReadGuestFavorites(request);

        if (favorites.Contains(productId))
        {
            favorites.Remove(productId);
            WriteGuestFavorites(response, favorites);
            return false;
        }
        else
        {
            favorites.Add(productId);
            WriteGuestFavorites(response, favorites);
            return true;
        }
    }

    public async Task MergeGuestFavorites(string userId, HttpRequest request, HttpResponse response)
    {
        ICollection<int> guestFavorites = ReadGuestFavorites(request);
        if (!guestFavorites.Any()) return;

        List<int> userFavorites = await this.context.UserFavoriteProducts.Where(x => x.UserId == userId)
            .Select(x => x.ProductId).ToListAsync();

        List<int> newFavorites = guestFavorites.Where(x => !userFavorites.Contains(x)).ToList();

        if (newFavorites.Any())
        {
            foreach (var productId in newFavorites)
            {
                this.context.UserFavoriteProducts.Add(new UserFavoriteProduct()
                {
                    UserId = userId,
                    ProductId = productId
                });
            }

            await this.context.SaveChangesAsync();
        }

        response.Cookies.Delete("GuestFavorites");
    }
}