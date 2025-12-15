namespace RandomShop.Models.Address;

public class AddressSnapshotModel
{
    public int? StreetNumber { get; set; }

    public string PostalCode { get; set; }

    public string AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public int? CountryId { get; set; }
}