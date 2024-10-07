using System.ComponentModel.DataAnnotations;

namespace RandomShop.Models.Variation
{
    public class VariationViewModel
    {
        [Required]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
