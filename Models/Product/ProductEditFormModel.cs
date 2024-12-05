using RandomShop.Data;
using RandomShop.Models.Category;
using RandomShop.Models.Promotion;
using RandomShop.Models.Variation;
using System.ComponentModel.DataAnnotations;

public class ProductEditFormModel
{
    public int Id { get; set; }

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

    //Make it list of ProductImageViewModel so Id's can be used in Edit View.
    public List<byte[]> Images = new List<byte[]>();
    public ICollection<IFormFile> NewAddedImages { get; set; } = new List<IFormFile>();


    //public List<int> ImagesForDelete { get; set; } = new List<int>();
    public string ImagesForDelete { get; set; }

    // public List<ProductImageViewModel> ExistingImages { get; set; } = new List<ProductImageViewModel>();

    public ICollection<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();

    public ICollection<PromotionViewModel> Promotions { get; set; } = new List<PromotionViewModel>();

    // public List<VariationViewModel> ExistingVariationOptions { get; set; } = new List<VariationViewModel>();
    public Dictionary<string, List<string>> ExistingVariationOptions { get; set; } = new Dictionary<string, List<string>>();

    public ICollection<VariationOptionViewModel> AllVariationOptions { get; set; } = new List<VariationOptionViewModel>();

    public List<VariationOptionFormViewModel> SelectedVariationOptions { get; set; } = new List<VariationOptionFormViewModel>();
}
