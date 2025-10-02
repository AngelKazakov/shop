namespace RandomShop.Models.Cart;

public class CartViewModel
{
    public ICollection<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

    public decimal GrandTotal => this.Items.Sum(item => item.TotalPrice);
}