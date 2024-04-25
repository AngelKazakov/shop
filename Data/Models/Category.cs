using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(DataConstants.productCategoryMaxLength, MinimumLength = DataConstants.productCategoryMinLength)]
        public string Name { get; set; }

        public int? ParentCategoryId { get; set; }

        public Category? ParentCategory { get; set; }

        public ICollection<Variation> Variations { get; set; } = new List<Variation>();

        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

        public ICollection<Category> Subcategories { get; set; } = new List<Category>();
    }
}
