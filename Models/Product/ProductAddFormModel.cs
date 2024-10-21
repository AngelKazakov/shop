using RandomShop.Data;
using RandomShop.Models.Category;
using System.ComponentModel.DataAnnotations;
using RandomShop.Models.Promotion;
using RandomShop.Models.Variation;

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
        [Range(DataConstants.Product.priceMin, DataConstants.Product.priceMax)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(DataConstants.Product.productItemSKUMaxLength, MinimumLength = DataConstants.Product.productItemSKUMinLength)]
        public string SKU { get; set; }

        public int CategoryId { get; set; }

        public int PromotionId { get; set; }

        [Required]
        [Range(DataConstants.Product.productItemQuantityMin, DataConstants.Product.productItemQuantityMax)]
        public int QuantityInStock { get; set; }

        public ICollection<IFormFile> Images { get; set; } = new List<IFormFile>();

        public ICollection<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();

        public List<VariationOptionViewModel> VariationOptions { get; set; } = new List<VariationOptionViewModel>();

        public List<VariationOptionFormViewModel> SelectedVariationOptions { get; set; } = new List<VariationOptionFormViewModel>();

        public ICollection<PromotionViewModel> Promotions { get; set; } = new List<PromotionViewModel>();
    }

    public class VariationOptionFormViewModel
    {
        public int VariationId { get; set; }
        public int VariationOptionId { get; set; }
    }
}
