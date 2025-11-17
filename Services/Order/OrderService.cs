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

    public async Task<CheckoutViewModel> GetCheckoutDataAsync(string userId)
    {
        ICollection<CartItemViewModel> cartItems = await this.cartService.GetCartItemsAsync(userId);
        var subTotal = cartItems.Sum(x => x.TotalPrice);

        var shippingMethods = await this.context.ShippingMethods.AsNoTracking().ToListAsync();
        var paymentTypes = await this.context.PaymentTypes.AsNoTracking().ToListAsync();
        var countries = await this.context.Countries.AsNoTracking().ToListAsync();

        var userAddresses = await context.UserAddresses
            .Include(ua => ua.Address)
            .ThenInclude(a => a.Country)
            .Where(ua => ua.UserId == userId)
            .ToListAsync();

        var savedAddresses = userAddresses.Select(ua => new AddressOptionViewModel()
        {
            AddressId = ua.AddressId,
            DisplayText =
                $"{ua.Address.StreetNumber}, {ua.Address.AddressLine1}, {ua.Address.PostalCode}, {ua.Address.Country.Name}",
            IsDefault = ua.IsDefault,
        }).ToList();


        var model = new CheckoutViewModel()
        {
            SubTotal = subTotal,
            ShippingMethods = shippingMethods,
            PaymentTypes = paymentTypes,
            SavedAddresses = savedAddresses,
            SelectedAddressId = savedAddresses.FirstOrDefault(a => a.IsDefault)?.AddressId,
            CartItems = cartItems,
        };

        return model;
    }
}