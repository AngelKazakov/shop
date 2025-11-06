namespace RandomShop.Services.Cart;

public interface IGuestFavoritesCookieService
{
    public ICollection<int> ReadGuestFavorites(HttpRequest request);

    public void WriteGuestFavorites(HttpResponse response, ICollection<int> productIds);

    public void ClearGuestFavorites(HttpResponse response);

    public bool ToggleFavorite(HttpRequest request, HttpResponse response, int productId);

    public Task MergeGuestFavorites(string userId, HttpRequest request, HttpResponse response);
}