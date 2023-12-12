namespace RandomShop.Data.Models
{
    public class ProductItem
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }

        public string SKU { get; set; }

        public int QuantityInStock { get; set; }

        public decimal Price { get; set; }


        public ICollection<ProductImage> ProductItemImages { get; set; } = new List<ProductImage>();

        public ICollection<ProductConfiguration> ProductConfigurations { get; set; } = new List<ProductConfiguration>();

        public ICollection<OrderLine> OrderLines { get; set; } = new List<OrderLine>();

    }
}
