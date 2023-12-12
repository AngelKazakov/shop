namespace RandomShop.Data.Models
{
    public class VariationOption 
    {
        public int Id { get; set; }

        public int VariationId { get; set; }

        public Variation Variation { get; set; }

        public string Value { get; set; }

        public ICollection<ProductConfiguration> ProductConfigurations { get; set; } = new List<ProductConfiguration>();
    }
}
