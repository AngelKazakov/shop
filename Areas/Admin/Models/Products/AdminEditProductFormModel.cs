using RandomShop.Models.Category;
using RandomShop.Models.Image;
using RandomShop.Models.Promotion;
using RandomShop.Models.Variation;

namespace RandomShop.Areas.Admin.Models;

public class AdminEditProductFormModel
{
    public int ProductItemId { get; set; }

    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public int QuantityInStock { get; set; }

    public decimal Price { get; set; }

    public decimal DiscountedPrice { get; set; }

    public int CategoryId { get; set; }

    public int? PromotionId { get; set; }

    public List<int> SelectedVariationOptionIds { get; set; } = new();

    public Dictionary<int, int?> SelectedVariationOptions { get; set; } = new();

    public Dictionary<string, List<string>> ExistingVariationOptions { get; set; } = new();

    public ICollection<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();

    public ICollection<PromotionViewModel> Promotions { get; set; } = new List<PromotionViewModel>();

    public ICollection<VariationOptionViewModel> AllVariationOptions { get; set; } = new List<VariationOptionViewModel>();

    public List<ProductImageViewModel> ExistingImages { get; set; } = new();

    public List<IFormFile> NewImages { get; set; } = new();

    public List<int> RemovedImageIds { get; set; } = new();
}
