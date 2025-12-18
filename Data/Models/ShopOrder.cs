using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class ShopOrder
    {
        public int Id { get; set; }

        [Required] public string UserId { get; set; }

        public User User { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        [Range(DataConstants.Address.StreetNumberMin, DataConstants.Address.StreetNumberMax)]
        public int StreetNumber { get; set; }

        [Required]
        [StringLength(DataConstants.Address.addressLineMaxLength,
            ErrorMessage = "Make sure you entered correctly your address.")]
        public string AddressLine1 { get; set; }

        [StringLength(DataConstants.Address.addressLineMaxLength,
            ErrorMessage = "Make sure you entered correctly your address.")]
        public string? AddressLine2 { get; set; }

        [Required] public int CountryId { get; set; }

        public Country Country { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "Postal code must be between 4 and 8 characters.")]
        public string? PostalCode { get; set; }
        //[Required]
        //[Range(DataConstants.Address.postalCodeMin, DataConstants.Address.postalCodeMax)]
        //public int PostalCode { get; set; }

        [Required] public int ShippingMethodId { get; set; }

        public ShippingMethod ShippingMethod { get; set; }

        [Required]
        [Range(DataConstants.orderlTotalMinValue, DataConstants.orderTotalMaxValue)]
        public decimal OrderTotal { get; set; }

        [Required] public int OrderStatusId { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public ICollection<UserPaymentMethod> UserPaymentMethods { get; set; } = new List<UserPaymentMethod>();

        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }
}