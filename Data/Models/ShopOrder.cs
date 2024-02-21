using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class ShopOrder
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public User User { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        public int ShippingAddressId { get; set; }

        public Address ShippingAddress { get; set; }

        [Required]
        public int ShippingMethodId { get; set; }

        public ShippingMethod ShippingMethod { get; set; }

        [Required]
        [Range(DataConstants.orderlTotalMinValue, DataConstants.orderTotalMaxValue)]
        public decimal OrderTotal { get; set; }

        [Required]
        public int OrderStatusId { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public ICollection<UserPaymentMethod> UserPaymentMethods { get; set; } = new List<UserPaymentMethod>();

        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }
}
