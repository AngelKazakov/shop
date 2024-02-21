using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class ProductImage
    {
        public int Id { get; init; }

        [Required]
        [StringLength(DataConstants.ProductImage.nameMaxLength, MinimumLength = DataConstants.ProductImage.nameMinLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(DataConstants.ProductImage.nameMaxLength, MinimumLength = DataConstants.ProductImage.nameMinLength)]
        public string UniqueName { get; set; }

        public string FullPath { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product Product { get; set; }
    }
}
