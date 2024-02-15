using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class PaymentType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(DataConstants.PaymentType.paymentTypeValueMaxLength, MinimumLength = DataConstants.PaymentType.paymentTypeValueMinLength)]
        [Display(Name = "Payment Type")]
        public string Value { get; set; }

        public ICollection<UserPaymentMethod> UserPaymentMethods { get; set; } = new List<UserPaymentMethod>();
    }
}
