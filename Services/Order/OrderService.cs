using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Models.Address;
using RandomShop.Models.Cart;
using RandomShop.Models.Order;
using RandomShop.Services.Cart;

namespace RandomShop.Services.Order;

public class OrderService : IOrderService
{
    private readonly ShopContext context;
    private readonly ICartService cartService;

    public OrderService(ShopContext context, ICartService cartService)
    {
        this.context = context;
        this.cartService = cartService;
    }

    public async Task<CartValidationResult> ValidateCartAsync(string userId)
    {
        var result = new CartValidationResult();

        var cart = await this.context.ShoppingCarts.Include(x => x.Items)
            .ThenInclude(x => x.ProductItem)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            result.Errors.Add("Your cart is empty");
            return result;
        }

        foreach (var cartItem in cart.Items)
        {
            if (cartItem.ProductItem == null)
            {
                result.Errors.Add($"Item with ID {cartItem.ProductItemId} no longer exists");
                continue;
            }

            var productName = cartItem.ProductItem.Product?.Name ?? "Unknown product";

            if (cartItem.ProductItem.QuantityInStock < cartItem.Quantity)
            {
                result.Errors.Add(
                    $"{productName} has only {cartItem.ProductItem.QuantityInStock} in stock.");
            }
        }

        return result;
    }

    public Dictionary<string, string> ValidateAddressSelection(CheckoutFormModel model)
    {
        var errors = new Dictionary<string, string>();

        if (model.UseNewAddress)
        {
            if (model.StreetNumber == null)
            {
                errors.Add(nameof(model.StreetNumber), "Street number is required.");
            }

            if (string.IsNullOrWhiteSpace(model.AddressLine1))
            {
                errors.Add(nameof(model.AddressLine1), "Address line 1 is required.");
            }

            if (!model.PostalCode.HasValue)
            {
                errors.Add(nameof(model.PostalCode), "Postal code is required.");
            }

            if (!model.CountryId.HasValue)
            {
                errors.Add(nameof(model.CountryId), "Country ID is required.");
            }
            else
            {
                if (!model.SelectedAddressId.HasValue)
                {
                    errors.Add(nameof(model.SelectedAddressId), "Please choose a saved address.");
                }
            }
        }

        return errors;
    }

    public async Task<CheckoutViewModel> GetCheckoutDataAsync(string userId)
    {
        ICollection<CartItemViewModel> cartItems = await this.cartService.GetCartItemsAsync(userId);
        decimal subTotal = cartItems.Sum(x => x.TotalPrice);

        var shippingMethods = await this.context.ShippingMethods.AsNoTracking().ToListAsync();
        var paymentTypes = await this.context.PaymentTypes.AsNoTracking().ToListAsync();
        var countries = await this.context.Countries.AsNoTracking().ToListAsync();

        var userAddresses = await context.UserAddresses
            .Include(ua => ua.Address)
            .ThenInclude(a => a.Country)
            .Where(ua => ua.UserId == userId)
            .ToListAsync();

        var savedAddresses = userAddresses
            .Select(ua => new AddressOptionViewModel
            {
                AddressId = ua.AddressId,
                DisplayText =
                    $"{ua.Address.StreetNumber}, {ua.Address.AddressLine1}, {ua.Address.PostalCode}, {ua.Address.Country.Name}",
                IsDefault = ua.IsDefault,
            })
            .ToList();

        decimal shippingPrice;
        if (subTotal >= DataConstants.freeShippingMinPrice)
        {
            shippingPrice = 0m;
        }
        else
        {
            shippingPrice = DataConstants.defaultShippingPrice;
        }

        var orderInfo = new OrderInfoViewModel
        {
            TotalPrice = subTotal,
            ShippingPrice = shippingPrice
        };

        var model = new CheckoutViewModel
        {
            SubTotal = subTotal,
            ShippingMethods = shippingMethods,
            PaymentTypes = paymentTypes,
            SavedAddresses = savedAddresses,
            SelectedAddressId = savedAddresses.FirstOrDefault(a => a.IsDefault)?.AddressId,
            Countries = countries,
            CartItems = cartItems,
            OrderInfo = orderInfo,
            UseNewAddress = false,
        };

        return model;
    }

    public async Task<int> PlaceOrderAsync(string userId, CheckoutFormModel model)
    {
        throw new NotImplementedException();
    }
}