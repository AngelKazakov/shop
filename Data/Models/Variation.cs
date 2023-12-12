namespace RandomShop.Data.Models
{
    public class Variation
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ProductCategoryId { get; set; }

        public ProductCategory ProductCategory { get; set; }

        public ICollection<VariationOption> VariationOptions { get; set; } = new List<VariationOption>();
    }
}
