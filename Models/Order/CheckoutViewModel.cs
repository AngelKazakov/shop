using RandomShop.Data.Models;
using RandomShop.Models.Address;
using RandomShop.Models.Cart;

namespace RandomShop.Models.Order;

public class CheckoutViewModel
{
    public ICollection<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

    public decimal SubTotal { get; set; }

    // public ICollection<AddressViewModel> Addresses { get; set; } = new List<AddressViewModel>();

    public ICollection<OrderInfoViewModel> OrderInfos { get; set; } = new List<OrderInfoViewModel>();

    public ICollection<ShippingMethod> ShippingMethods { get; set; } = new List<ShippingMethod>();

    public ICollection<PaymentType> PaymentTypes { get; set; } = new List<PaymentType>();

    public ICollection<AddressOptionViewModel> SavedAddresses { get; set; } = new List<AddressOptionViewModel>();

    public int? SelectedAddressId { get; set; }

    //Prefilled values for the form (optional)
    //public int? StreetNumber { get; set; }

    //public string? AddressLine1 { get; set; }

    //public string? AddressLine2 { get; set; }

    //public int? PostalCode { get; set; }

    //public int? CountryId { get; set; }

    //public int? PaymentTypeId { get; set; }
}