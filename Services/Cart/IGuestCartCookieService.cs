using RandomShop.Models.Cookie;

namespace RandomShop.Services.Cart;

public interface IGuestCartCookieService
{
    public void WriteGuestCart(HttpResponse response, List<CartCookieItem> items);
}