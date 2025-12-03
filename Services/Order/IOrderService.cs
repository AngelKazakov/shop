using RandomShop.Models.Cart;
using RandomShop.Models.Order;

namespace RandomShop.Services.Order;

public interface IOrderService
{
    public Task<CartValidationResult> ValidateCartAsync(string userId);

    public Dictionary<string, string> ValidateAddressSelection(CheckoutFormModel model);

    public Task<CheckoutViewModel> GetCheckoutDataAsync(string userId);

    public Task<int> PlaceOrderAsync(string userId, CheckoutFormModel model);
}