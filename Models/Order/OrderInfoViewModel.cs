namespace RandomShop.Models.Order;

public class OrderInfoViewModel
{
    public decimal TotalPrice { get; set; }

    public decimal ShippingPrice { get; set; }

    public decimal GrandTotal => TotalPrice + ShippingPrice;
}