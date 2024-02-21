using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class ShoppingCartItem
    {
        public int Id { get; set; }

        [Required]
        public int ShoppingCartId { get; set; }

        public ShoppingCart ShoppingCart { get; set; }

        [Required]
        public int ProductItemId { get; set; }

        public ProductItem ProductItem { get; set; }

        [Required]
        [Range(DataConstants.shoppingCartItemQuantityMinValue, DataConstants.shoppingCartItemQuantityMaxValue)]
        public int Quantity { get; set; }
    }
}
