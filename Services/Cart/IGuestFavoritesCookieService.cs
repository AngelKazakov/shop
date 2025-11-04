namespace RandomShop.Services.Cart;

public interface IGuestFavoritesCookieService
{
    public ICollection<int> ReadGuestFavorites(HttpRequest request);

    public void WriteGuestFavorites(HttpResponse response, ICollection<int> productIds);
}