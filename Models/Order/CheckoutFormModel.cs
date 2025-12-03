using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using RandomShop.Data;

namespace RandomShop.Models.Order;

public class CheckoutFormModel
{
    [Required] 
    public int SelectedShippingMethodId { get; set; }

    [Required] 
    public int SelectedAddressId { get; set; }

    [Required]
    public int PaymentTypeId { get; set; }

    public bool UseNewAddress { get; set; }

    public int? CountryId { get; set; }

    [Required]
    [Range(DataConstants.Address.StreetNumberMin, DataConstants.Address.StreetNumberMax)]
    public int? StreetNumber { get; set; }

    [Required]
    [StringLength(DataConstants.Address.addressLineMaxLength)]
    public string? AddressLine1 { get; set; }

    [StringLength(DataConstants.Address.addressLineMaxLength)]
    public string? AddressLine2 { get; set; }

    [Required]
    [Range(DataConstants.Address.postalCodeMin, DataConstants.Address.postalCodeMax)]
    public int? PostalCode { get; set; }

    // public string? Provider { get; set; }

    // public decimal OrderTotal { get; set; }
}