using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class VariationOption
    {
        public int Id { get; set; }

        [Required]
        public int VariationId { get; set; }

        public Variation Variation { get; set; }

        [Required]
        [StringLength(DataConstants.variationOptionValueMaxLength, MinimumLength = DataConstants.variationOptionValueMinLength)]
        public string Value { get; set; }

        public ICollection<ProductConfiguration> ProductConfigurations { get; set; } = new List<ProductConfiguration>();
    }
}
