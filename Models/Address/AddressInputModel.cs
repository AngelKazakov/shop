using System.ComponentModel.DataAnnotations;
using RandomShop.Data;

namespace RandomShop.Models.Address;

public class AddressInputModel
{
    public int? CountryId { get; set; }

    [Range(DataConstants.Address.StreetNumberMin, DataConstants.Address.StreetNumberMax)]
    public int? StreetNumber { get; set; }

     [StringLength(DataConstants.Address.addressLineMaxLength, MinimumLength = DataConstants.Address.addressLineMinLength)]
    public string? AddressLine1 { get; set; }

    [StringLength(DataConstants.Address.addressLineMaxLength,
        MinimumLength = DataConstants.Address.addressLineMinLength)]
    public string? AddressLine2 { get; set; }

    [StringLength(8, MinimumLength = 4, ErrorMessage = "Postal code must be between 4 and 8 characters.")]
    public string? PostalCode { get; set; }
}