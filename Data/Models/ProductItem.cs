using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class ProductItem
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product Product { get; set; }

        [Required]
        [StringLength(DataConstants.Product.productItemSKUMaxLength, MinimumLength = DataConstants.Product.productItemSKUMinLength)]
        public string SKU { get; set; }

        [Required]
        [Range(DataConstants.Product.productItemQuantityMin, DataConstants.Product.productItemQuantityMax)]
        public int QuantityInStock { get; set; }

        [Required]
        [Range(DataConstants.Product.priceMin, DataConstants.Product.priceMax)]
        public decimal Price { get; set; }

        public ICollection<ProductImage> ProductItemImages { get; set; } = new List<ProductImage>();

        public ICollection<ProductConfiguration> ProductConfigurations { get; set; } = new List<ProductConfiguration>();

        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();

    }
}
