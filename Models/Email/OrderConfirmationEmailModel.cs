namespace RandomShop.Models.Email;

public class OrderConfirmationEmailModel
{
    public string CustomerName { get; set; } = string.Empty;

    public string OrderNumber { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal Shipping { get; set; }

    public decimal Total { get; set; }

    public ICollection<OrderConfirmationEmailItemModel> Items { get; set; } = new List<OrderConfirmationEmailItemModel>();
}
