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

        //Use it in the Checkout view to show the user what the shipping price will cost.
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
        using var transaction = await this.context.Database.BeginTransactionAsync();

        try
        {
            ICollection<CartItemViewModel> cartItems = await this.cartService.GetCartItemsAsync(userId);
            //Set default const status name and use it instead of hard coded one...
            int defaultStatusId = await this.context.OrderStatuses.Where(s => s.Status == "Pending Payment")
                .Select(s => s.Id).FirstOrDefaultAsync();

            AddressSnapshotModel addressSnapshotModel = await this.addressService.HandleOrderAddressAsync(userId,
                model.Address,
                model.SaveShippingAddress,
                model.SelectedAddressId, model.UseNewAddress);

            //Create enum for statuses and set
            // This line tells EF Core to store the enum value as a string
            // modelBuilder.Entity<ShopOrder>()
            //     .Property(o => o.Status)
            //     .HasConversion<string>(); // Use HasConversion<string>()

            decimal subtotal = cartItems.Sum(i => i.UnitPrice * i.Quantity);
            // later calculate properly   shippingPrice and add it to order total and visualize in the UI.
            decimal orderTotal = subtotal; // or subtotal + shippingPrice

            ShopOrder shopOrder = new ShopOrder()
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                StreetNumber = addressSnapshotModel.StreetNumber.Value,
                AddressLine1 = addressSnapshotModel.AddressLine1,
                AddressLine2 = addressSnapshotModel.AddressLine2,
                CountryId = addressSnapshotModel.CountryId.Value,
                PostalCode = addressSnapshotModel.PostalCode,
                ShippingMethodId = model.SelectedShippingMethodId,
                OrderTotal = orderTotal,
                OrderStatusId = defaultStatusId,
            };

            await this.context.ShopOrders.AddAsync(shopOrder);

            var orderLines = new List<OrderLine>();

            foreach (var cartItem in cartItems)
            {
                OrderLine orderLine = new OrderLine()
                {
                    Price = cartItem.UnitPrice,
                    Quantity = cartItem.Quantity,
                    ProductItemId = cartItem.ProductItemId,
                    ShopOrder = shopOrder,
                };

                orderLines.Add(orderLine);
            }

            shopOrder.OrderLines = orderLines;
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
            ShoppingCart cartToClear = await this.cartService.GetOrCreateCartAsync(userId);

            if (cartToClear != null)
            {
                await this.cartService.ClearCart(userId);
            }

            await transaction.CommitAsync();

            return shopOrder.Id;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw; // Re-throw the exception to notify the controller/caller
        }
    }
}