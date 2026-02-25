using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
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
        await using var transaction =
            await this.context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            ICollection<CartItemViewModel> cartItems = await this.cartService.GetCartItemsAsync(userId);

            if (cartItems == null || cartItems.Count == 0)
            {
                throw new InvalidOperationException("Cart is empty.");
            }

            //Set default const status name and use it instead of hard coded one...
            int defaultStatusId = await this.context.OrderStatuses
                .Where(s => s.Status == "Pending Payment")
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            if (defaultStatusId == 0)
            {
                throw new InvalidOperationException("Order status 'Pending Payment' was not found.");
            }

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
                OrderDate = DateTime.UtcNow,
                StreetNumber = addressSnapshotModel.StreetNumber.Value,
                AddressLine1 = addressSnapshotModel.AddressLine1,
                AddressLine2 = addressSnapshotModel.AddressLine2,
                CountryId = addressSnapshotModel.CountryId.Value,
                PostalCode = addressSnapshotModel.PostalCode,
                ShippingMethodId = model.SelectedShippingMethodId,
                OrderTotal = orderTotal,
                OrderStatusId = defaultStatusId,
            };

            List<OrderLine> orderLines = cartItems.Select(ci => new OrderLine()
            {
                Price = ci.UnitPrice,
                Quantity = ci.Quantity,
                ProductItemId = ci.ProductItemId,
                ShopOrder = shopOrder,
            }).ToList();

            shopOrder.OrderLines = orderLines;

            // Stock check & subtract
            Dictionary<int, int> needed = orderLines
                .GroupBy(ol => ol.ProductItemId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            List<int> ids = needed.Keys.ToList();

            List<ProductItem> productItems = await this.context.ProductItems
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            if (productItems.Count != ids.Count)
            {
                throw new InvalidOperationException("One or more products in the cart no longer exist.");
            }

            foreach (var pi in productItems)
            {
                int reqQty = needed[pi.Id];
                if (pi.QuantityInStock < reqQty)
                {
                    throw new InvalidOperationException(
                        $"Not enough stock for ProductItem {pi.Id}. Requested {reqQty}, available {pi.QuantityInStock}");
                }
            }

            foreach (var pi in productItems)
            {
                pi.QuantityInStock -= needed[pi.Id];
            }

            // Persist order + lines (EF sees the relationship via ShopOrder reference)
            var tmp = Guid.NewGuid().ToString("N")[..12];
            shopOrder.OrderNumber = $"TMP-{tmp}";
            await this.context.ShopOrders.AddAsync(shopOrder);

            await this.context.SaveChangesAsync();

            shopOrder.OrderNumber = $"RS-{shopOrder.Id:D8}";

            await this.context.SaveChangesAsync();

            // Clear cart (ideally ClearCart shouldn't create a cart)
            await this.cartService.ClearCart(userId);

            await transaction.CommitAsync();
            return shopOrder.Id;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw; // Re-throw the exception to notify the controller/caller
        }
    }

    public async Task<OrderConfirmationViewModel?> GetOrderDetailsAsync(int orderId, string userId)
    {
        OrderConfirmationViewModel? model = await this.context.ShopOrders
            .AsNoTracking()
            .Where(o => o.Id == orderId && o.UserId == userId)
            .Select(o => new OrderConfirmationViewModel
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                Status = o.OrderStatus.Status,
                ShippingMethodName = o.ShippingMethod.Name,
                AddressDisplay =
                    o.StreetNumber + " " + o.AddressLine1 +
                    (string.IsNullOrWhiteSpace(o.AddressLine2) ? "" : ", " + o.AddressLine2) +
                    ", " + o.PostalCode,

                Items = o.OrderLines.Select(ol => new OrderConfirmationItemViewModel
                {
                    ProductItemId = ol.ProductItemId,
                    ProductName = ol.ProductItem.Product.Name,
                    Quantity = ol.Quantity,
                    UnitPrice = ol.Price,
                    LineTotal = ol.Price * ol.Quantity
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (model == null) return null;

        model.Subtotal = model.Items.Sum(i => i.LineTotal);
        // model.ShippingPrice = ... later
        return model;
    }
}