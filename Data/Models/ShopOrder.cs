namespace RandomShop.Data.Models
{
    public class ShopOrder
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public DateTime OrderDate { get; set; }

        public int UserPaymentMethodId { get; set; }

        public UserPaymentMethod PaymentMethod { get; set; }

        public int AddressId { get; set; }

        public Address ShippingAddress { get; set; }

        public int ShippingMethodId { get; set; }

        public ShippingMethod ShippingMethod { get; set; }

        public decimal OrderTotal { get; set; }

        public int OrderStatusId { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public ICollection<UserPaymentMethod> UserPaymentMethods { get; set; } = new List<UserPaymentMethod>();

        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
    }
}
