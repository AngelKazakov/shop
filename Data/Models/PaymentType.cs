namespace RandomShop.Data.Models
{
    public class PaymentType
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public ICollection<UserPaymentMethod> UserPaymentMethods { get; set; } = new List<UserPaymentMethod>();
    }
}
