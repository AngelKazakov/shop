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

        [Required]
        [Range(DataConstants.Product.priceMin, DataConstants.Product.priceMax, ErrorMessage = DataConstants.Product.productPriceErrorMessage)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(DataConstants.Product.productItemSKUMaxLength, MinimumLength = DataConstants.Product.productItemSKUMinLength, ErrorMessage = DataConstants.Product.productSKUErrorMessage)]
        public string SKU { get; set; }

        [Required]
        [Range(DataConstants.Product.productItemQuantityMin, DataConstants.Product.productItemQuantityMax, ErrorMessage = DataConstants.Product.productQuantityErrorMessage)]
        public int QuantityInStock { get; set; }

        [Required]
        public ProductCategoryFormModel Category { get; set; }

        [Required]
        public ProductVariationFormModel Variation { get; set; }

        public ICollection<IFormFile> Images { get; set; } = new List<IFormFile>();

        public ICollection<ProductCategoryFormModel> Categories { get; set; } = new List<ProductCategoryFormModel>();

        public ICollection<ProductVariationFormModel> Variations { get; set; } = new List<ProductVariationFormModel>();
    }
}
