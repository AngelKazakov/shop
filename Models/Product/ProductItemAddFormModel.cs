using RandomShop.Data;
using RandomShop.Models.Category;
using RandomShop.Models.Variation;
using System.ComponentModel.DataAnnotations;

namespace RandomShop.Models.Product
{
    public class ProductItemAddFormModel
    {
        [Required]
        [Range(DataConstants.Product.priceMin, DataConstants.Product.priceMax, ErrorMessage = DataConstants.Product.productPriceErrorMessage)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(DataConstants.Product.productItemSKUMaxLength, MinimumLength = DataConstants.Product.productItemSKUMinLength, ErrorMessage = DataConstants.Product.productSKUErrorMessage)]
        public string SKU { get; set; }

        [Required]
        [Range(DataConstants.Product.productItemQuantityMin, DataConstants.Product.productItemQuantityMax, ErrorMessage = DataConstants.Product.productQuantityErrorMessage)]
        public int QuantityInStock { get; set; }

        public ICollection<IFormFile> Images { get; set; } = new List<IFormFile>();

        public ICollection<CategoryFormModel> Categories { get; set; } = new List<CategoryFormModel>();

        public ICollection<VariationViewModel> Variations { get; set; } = new List<VariationViewModel>();
    }
}
