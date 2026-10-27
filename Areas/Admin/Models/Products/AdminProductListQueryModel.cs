namespace RandomShop.Areas.Admin.Models;

public class AdminProductListQueryModel
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public string? SearchTerm { get; set; }

    public int? CategoryId { get; set; }

    public int? PromotionId { get; set; }

    public bool? InStockOnly { get; set; }

    public string StockFilter { get; set; } = "all";

    public string PromotionFilter { get; set; } = "all";

    public string SortBy { get; set; } = "Newest";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = DefaultPageSize;

    public int TotalItems { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)this.TotalItems / this.PageSize);

    public ICollection<AdminProductListItemViewModel> Items { get; set; } = new List<AdminProductListItemViewModel>();
}
