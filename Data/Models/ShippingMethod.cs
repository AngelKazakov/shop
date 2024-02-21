using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class ShippingMethod
    {
        public int Id { get; set; }

        [Required]
        [StringLength(DataConstants.shippingMethodNameMaxLength, MinimumLength = DataConstants.shippingMethodNameMinLength)]
        public string Name { get; set; }

        //[Required]
        [Range(DataConstants.shippingMethodMinPrice, DataConstants.shippingMethodMaxPrice)]
        public decimal Price { get; set; }

        public ICollection<ShopOrder> ShopOrders { get; set; } = new List<ShopOrder>();
    }
}
