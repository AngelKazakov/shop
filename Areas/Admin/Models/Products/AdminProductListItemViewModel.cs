namespace RandomShop.Areas.Admin.Models;

public class AdminProductListItemViewModel
{
    public int ProductItemId { get; set; }

    public int ProductId { get; set; }

    public string Name { get; set; }

    public string SKU { get; set; }

    public string CategoryName { get; set; }

    public decimal Price { get; set; }

    public decimal? DiscountedPrice { get; set; }

    public int Quantity { get; set; }

    public bool HasPromotion { get; set; }

    public DateTime CreatedOn { get; set; }
}