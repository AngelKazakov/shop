namespace RandomShop.Models.Order;

public class OrderConfirmationViewModel
{
    public int OrderId { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; }

    public string ShippingMethodName { get; set; }

    public decimal ShippingPrice { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Total { get; set; }

    public int StreetNumber { get; set; }

    public string AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string PostalCode { get; set; }

    public string CountryName { get; set; }

    public string AddressDisplay { get; set; }

    public List<OrderConfirmationItemViewModel> Items { get; set; } = new List<OrderConfirmationItemViewModel>();
}
