namespace RandomShop.Data.Models
{
    public class Promotion
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int DiscountRate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public ICollection<PromotionCategory> PromotionCategories { get; set; } = new List<PromotionCategory>();
    }
}
