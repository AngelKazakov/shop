using RandomShop.Models.Cart;
using RandomShop.Models.Cookie;

namespace RandomShop.Services.Cart;

public interface IGuestCartCookieService
{
    public void WriteGuestCart(HttpResponse response, List<CartCookieItem> items);
    public List<CartCookieItem> ReadGuestCart(HttpRequest request);

    public void AddOrUpdateGuestCart(HttpRequest request, HttpResponse response, int productItemId, int quantity);

    public Task<CartViewModel> GetGuestCart(List<CartCookieItem>? guestItems);
}