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

    public async Task<CheckoutViewModel> GetCheckoutDataAsync(string userId, int? selectedShippingMethodId = null)
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

        int resolvedShippingMethodId = selectedShippingMethodId
                                       ?? shippingMethods.FirstOrDefault()?.Id
                                       ?? 0;

        decimal shippingPrice = CalculateShippingPrice(subTotal, resolvedShippingMethodId, shippingMethods);

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
            SelectedShippingMethodId = resolvedShippingMethodId,
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

            if (cartItems.Count == 0)
            {
                throw new InvalidOperationException("Cart is empty.");
            }

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

            decimal subtotal = cartItems.Sum(i => i.UnitPrice * i.Quantity);

            ShippingMethod shippingMethod = await this.context.ShippingMethods
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(sm => sm.Id == model.SelectedShippingMethodId)
                                            ?? throw new InvalidOperationException(
                                                "Selected shipping method was not found.");

            decimal shippingPrice = CalculateShippingPrice(
                subtotal,
                model.SelectedShippingMethodId,
                new List<ShippingMethod> { shippingMethod });

            decimal orderTotal = subtotal + shippingPrice;

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

            var suffix = GenerateOrderNumber();
            shopOrder.OrderNumber = suffix;

            await this.context.ShopOrders.AddAsync(shopOrder);
            await this.context.SaveChangesAsync();

            await this.cartService.ClearCart(userId);

            await transaction.CommitAsync();
            return shopOrder.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
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
                Total = o.OrderTotal,
                StreetNumber = o.StreetNumber,
                AddressLine1 = o.AddressLine1,
                AddressLine2 = o.AddressLine2,
                PostalCode = o.PostalCode,
                CountryName = o.Country.Name,
                AddressDisplay =
                    o.StreetNumber + " " + o.AddressLine1 +
                    (string.IsNullOrWhiteSpace(o.AddressLine2) ? "" : ", " + o.AddressLine2) +
                    ", " + o.PostalCode + ", " + o.Country.Name,

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
        model.ShippingPrice = Math.Max(0m, model.Total - model.Subtotal);
        return model;
    }

    public async Task<ICollection<OrderHistoryViewModel>> GetOrderHistoryByUserIdAsync(string userId)
    {
        List<OrderHistoryViewModel> userOrders = await this.context.ShopOrders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderHistoryViewModel
            {
                OrderId = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                Status = o.OrderStatus.Status,
                Total = o.OrderTotal,
                ItemCount = o.OrderLines.Sum(ol => ol.Quantity)
            })
            .ToListAsync();

        return userOrders;
    }

    private static decimal CalculateShippingPrice(
        decimal subtotal,
        int selectedShippingMethodId,
        IReadOnlyCollection<ShippingMethod> shippingMethods)
    {
        if (subtotal >= DataConstants.freeShippingMinPrice)
        {
            return 0m;
        }

        ShippingMethod? selectedShippingMethod = shippingMethods
            .FirstOrDefault(sm => sm.Id == selectedShippingMethodId);

        return selectedShippingMethod?.Price ?? DataConstants.defaultShippingPrice;
    }

    private static string GenerateOrderNumber()
    {
        var suffix = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
        return $"RS-{suffix}";
    }
}
