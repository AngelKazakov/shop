using RandomShop.Models.Category;
using System.ComponentModel.DataAnnotations;

namespace RandomShop.Models.Variation
{
    public class VariationAddFormModel
    {
        [Required]
        public string Name { get; set; }

        public string Value { get; set; }

        public int CategoryId { get; set; }

        public ICollection<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();

        public ICollection<VariationOptionAddFormModel> VariationOptionAddFormModels { get; set; } = new List<VariationOptionAddFormModel>();
    }
}
