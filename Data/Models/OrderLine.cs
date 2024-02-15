using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class OrderLine
    {
        public int Id { get; set; }

        [Required]
        public int ProductItemId { get; set; }

        public ProductItem ProductItem { get; set; }

        [Required]
        public int ShopOrderId { get; set; }

        public ShopOrder ShopOrder { get; set; }

        [Required]
        [Range(DataConstants.OrderLine.quantityMin, DataConstants.OrderLine.quantityMax)]
        public int Quantity { get; set; }

        //Check if decimal works properly
        [Required]
        [Range(DataConstants.OrderLine.priceMin, DataConstants.OrderLine.priceMax)]
        public decimal Price { get; set; }

        public ICollection<UserReview> UserReviews { get; set; } = new List<UserReview>();
    }
}
