namespace RandomShop.Models.Order;

public class OrderConfirmationItemViewModel
{
    public int ProductItemId { get; set; }

    public string ProductName { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }

    //Add images later.
}