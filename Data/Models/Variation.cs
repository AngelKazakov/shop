using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class Variation
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int ProductCategoryId { get; set; }

        public ProductCategory ProductCategory { get; set; }

        public ICollection<VariationOption> VariationOptions { get; set; } = new List<VariationOption>();
    }
}
