using System.ComponentModel.DataAnnotations;

namespace RandomShop.Models.Variation
{
    public class VariationOptionAddFormModel
    {
        [Required]
        public string Value { get; set; }
    }
}
