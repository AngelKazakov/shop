namespace RandomShop.Models.Email;

public class OrderConfirmationEmailItemModel
{
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
