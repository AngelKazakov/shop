using RandomShop.Models.Cart;
using RandomShop.Models.Order;

namespace RandomShop.Services.Order;

public interface IOrderService
{
    public Task<CartValidationResult> ValidateCartAsync(string userId);

    public Task<CheckoutViewModel> GetCheckoutDataAsync(string userId, int? selectedShippingMethodId = null);

    public Task<int> PlaceOrderAsync(string userId, CheckoutFormModel model);

    Task<OrderConfirmationViewModel?> GetOrderDetailsAsync(int orderId, string userId);
}
