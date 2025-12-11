using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Models.Address;
using RandomShop.Models.Cart;
using RandomShop.Models.Order;
using RandomShop.Services.Address;
using RandomShop.Services.Cart;

namespace RandomShop.Services.Order;

public class OrderService : IOrderService
{
    private readonly ShopContext context;
    private readonly ICartService cartService;
    private readonly IAddressService addressService;

    public OrderService(ShopContext context, ICartService cartService, IAddressService addressService)
    {
        this.context = context;
        this.cartService = cartService;
        this.addressService = addressService;
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
        ICollection<CartItemViewModel> cartItems = await this.cartService.GetCartItemsAsync(userId);

        int defaultStatusId = await this.context.OrderStatuses.Where(s => s.Status == "Pending Payment")
            .Select(s => s.Id).FirstOrDefaultAsync();

        var adressId = await this.addressService.HandleOrderAddressAsync(userId, model.AddressInputModel,
            model.SaveShippingAddress,
            2, model.UseNewAddress);

        //Create enum for statuses and set
        // This line tells EF Core to store the enum value as a string
        // modelBuilder.Entity<ShopOrder>()
        //     .Property(o => o.Status)
        //     .HasConversion<string>(); // Use HasConversion<string>()


        ShopOrder shopOrder = new ShopOrder()
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            StreetNumber = model.AddressInputModel.StreetNumber.Value,
            AddressLine1 = model.AddressInputModel.AddressLine1,
            AddressLine2 = model.AddressInputModel.AddressLine2,
            CountryId = model.AddressInputModel.CountryId.Value,
            PostalCode = model.AddressInputModel.PostalCode,
            ShippingMethodId = model.SelectedShippingMethodId,
            OrderTotal = model.OrderTotal,
            OrderStatusId = defaultStatusId,
        };

        await this.context.ShopOrders.AddAsync(shopOrder);

        var orderLines = new List<OrderLine>();

        foreach (var cartItem in cartItems)
        {
            OrderLine orderLine = new OrderLine()
            {
                Price = cartItem.TotalPrice,
                Quantity = cartItem.Quantity,
                ProductItemId = cartItem.ProductItemId,
                ShopOrder = shopOrder,
            };

            orderLines.Add(orderLine);
        }

        await this.context.OrderLines.AddRangeAsync(orderLines);
        await this.context.SaveChangesAsync();


        foreach (var orderLine in shopOrder.OrderLines)
        {
            ProductItem? productItem = await this.context.ProductItems.FindAsync(orderLine.ProductItemId);

            if (productItem != null)
            {
                productItem.QuantityInStock -= orderLine.Quantity;
            }
        }

        await this.context.SaveChangesAsync();

        //  Clear user's shopping cart here.

        ShoppingCart cart = await this.cartService.GetOrCreateCartAsync(userId);

        if (cart != null)
        {
            await this.cartService.ClearCart(userId);
        }

        throw new NotImplementedException();
    }
}