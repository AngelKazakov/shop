using RandomShop.Data;
using System.ComponentModel.DataAnnotations;

namespace RandomShop.Models.Product
{
    public class ProductAddFormModel
    {
        [Required]
        [StringLength(DataConstants.Product.nameMaxLength, MinimumLength = DataConstants.Product.nameMinLength, ErrorMessage = DataConstants.Product.productNameErrorMessage)]
        public string Name { get; set; }

        [Required]
        [StringLength(DataConstants.Product.descriptionMaxLength, MinimumLength = DataConstants.Product.descriptionMinLength, ErrorMessage = DataConstants.Product.productDescriptionErrorMessage)]
        public string Description { get; set; }

        public ICollection<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
