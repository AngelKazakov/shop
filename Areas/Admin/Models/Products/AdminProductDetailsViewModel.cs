using RandomShop.Models.Image;

namespace RandomShop.Areas.Admin.Models;

public class AdminProductDetailsViewModel
{
    public int ProductItemId { get; set; }

    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal DiscountedPrice { get; set; }

    public int QuantityInStock { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? PromotionName { get; set; }

    public DateTime CreatedOnDate { get; set; }

    public ICollection<ProductImageViewModel> Images { get; set; } = new List<ProductImageViewModel>();
}
