using System.ComponentModel.DataAnnotations;

namespace RandomShop.Models.Variation
{
    public class VariationAddFormModel
    {
        [Required]
        public string Name { get; set; }

        public ICollection<VariationOptionAddFormModel> VariationOptionAddFormModels { get; set; } = new List<VariationOptionAddFormModel>();
    }
}
