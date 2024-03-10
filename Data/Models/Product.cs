using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class Product
    {
        public int Id { get; init; }

        [Required]
        [StringLength(DataConstants.Product.nameMaxLength, MinimumLength = DataConstants.Product.nameMinLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(DataConstants.Product.descriptionMaxLength, MinimumLength = DataConstants.Product.descriptionMinLength)]
        public string Description { get; set; }

        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

        public ICollection<ProductItem> ProductItems { get; set; } = new List<ProductItem>();

        public ICollection<ProductPromotion> ProductPromotions { get; set; } = new List<ProductPromotion>();

        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    }
}
