namespace RandomShop.Data.Models
{
    public class Address
    {
        public int Id { get; set; }

        public int StreetNumber { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public int CountryId { get; set; }

        public Country Country { get; set; }

        public int PostalCode { get; set; }

        public ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
    }
}
