using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class OrderStatus
    {
        public int Id { get; set; }

        [Required]
        [StringLength(DataConstants.statusMaxLength)]
        public string Status { get; set; }

        public ICollection<ShopOrder> ShopOrders { get; set; } = new List<ShopOrder>();
    }
}
