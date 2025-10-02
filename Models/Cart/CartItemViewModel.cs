namespace RandomShop.Models.Cart;

public class CartItemViewModel
{
    public int ProductItemId { get; set; }

    public string ProductName { get; set; }

    //Add image here...
    //public string ImageUrl = {get; set;}

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice => this.UnitPrice * this.Quantity;
}