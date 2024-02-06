namespace RandomShop.Data.Models
{
    public class UserAddress
    {
        public string UserId { get; set; }

        public User User { get; set; }

        public int AddressId { get; set; }

        public Address Address { get; set; }

        public bool IsDefault { get; set; }
    }
}
