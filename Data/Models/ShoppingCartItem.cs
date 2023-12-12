namespace RandomShop.Data.Models
{
    public class ShoppingCartItem
    {
        public int Id { get; set; }

        public int ShoppingCartId { get; set; }

        public ShoppingCart ShoppingCart { get; set; }

        public int ProductItemId { get; set; }

        public ProductItem ProductItem { get; set; }

        public int Quantity { get; set; }
    }
}
