namespace RandomShop.Data.Models
{
    public class OrderLine
    {
        public int Id { get; set; }

        public int ProductItemId { get; set; }

        public ProductItem ProductItem { get; set; }

        public int ShopOrderId { get; set; }

        public ShopOrder ShopOrder { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public ICollection<UserReview> UserReviews { get; set; } = new List<UserReview>();
    }
}
