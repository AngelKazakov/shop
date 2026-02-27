namespace RandomShop.Models.Order;

public class OrderHistoryViewModel
{
    public int OrderId { get; set; }
    
    public string OrderNumber { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public string Status { get; set; }

    public decimal Total { get; set; }

    public int ItemCount { get; set; }
}
