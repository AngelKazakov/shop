using System.ComponentModel.DataAnnotations;

namespace RandomShop.Models.Category
{
    public class CategoryViewModel
    {
        [Required]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
