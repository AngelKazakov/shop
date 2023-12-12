namespace RandomShop.Data.Models
{
    public class UserReview
    {


        public int Id { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public int OrderedProductId { get; set; }

        public double RatingValue { get; set; }

        public string Comment { get; set; }

        public int OrderLineId { get; set; }

        public OrderLine OrderLine { get; set; }
    }
}
