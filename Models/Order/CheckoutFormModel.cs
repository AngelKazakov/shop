using System.ComponentModel.DataAnnotations;
using RandomShop.Models.Address;

namespace RandomShop.Models.Order;

public class CheckoutFormModel
{
    public int SelectedShippingMethodId { get; set; }

    public int? SelectedAddressId { get; set; }

    public int SelectedPaymentTypeId { get; set; }

    public bool UseNewAddress { get; set; }

    public AddressInputModel? Address { get; set; }

    public bool SaveShippingAddress { get; set; }

    public decimal OrderTotal { get; set; }

    // public string? Provider { get; set; }
}