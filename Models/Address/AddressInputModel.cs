using System.ComponentModel.DataAnnotations;
using RandomShop.Data;

namespace RandomShop.Models.Address;

public class AddressInputModel
{
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
}