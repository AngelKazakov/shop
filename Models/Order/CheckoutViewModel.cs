using RandomShop.Data.Models;
using RandomShop.Models.Address;
using RandomShop.Models.Cart;

namespace RandomShop.Models.Order;

public class CheckoutViewModel
{
    public ICollection<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

    public decimal SubTotal { get; set; }

    public OrderInfoViewModel OrderInfo { get; set; }

    public ICollection<ShippingMethod> ShippingMethods { get; set; } = new List<ShippingMethod>();

    public int SelectedShippingMethodId { get; set; }

    public ICollection<PaymentType> PaymentTypes { get; set; } = new List<PaymentType>();

    public int SelectedPaymentTypeId { get; set; }

    public ICollection<AddressOptionViewModel> SavedAddresses { get; set; } = new List<AddressOptionViewModel>();

    public int? SelectedAddressId { get; set; }

    public ICollection<Country> Countries { get; set; } = new List<Country>();

    public bool UseNewAddress { get; set; }

    public int? StreetNumber { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public int? PostalCode { get; set; }

    public int? CountryId { get; set; }
}