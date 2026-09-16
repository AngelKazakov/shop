using RandomShop.Models.Category;
using RandomShop.Models.Promotion;
using RandomShop.Models.Variation;

namespace RandomShop.Areas.Admin.Models;

public class AdminCreateProductFormModel
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public int QuantityInStock { get; set; }

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public int? PromotionId { get; set; }

    public List<int> SelectedVariationOptionIds { get; set; } = new();

    public List<IFormFile> Images { get; set; } = new();

    public ICollection<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();

    public ICollection<PromotionViewModel> Promotions { get; set; } = new List<PromotionViewModel>();

    public ICollection<VariationOptionViewModel> VariationOptions { get; set; } = new List<VariationOptionViewModel>();
}