using System.ComponentModel.DataAnnotations;

namespace RandomShop.Models.Category
{
    public class CategoryFormModel
    {
        [Required]
        public string Name { get; set; }
    }
}
