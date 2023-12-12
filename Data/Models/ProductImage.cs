namespace RandomShop.Data.Models
{
    public class ProductImage
    {
        public int Id { get; init; }

        public string Name { get; set; }

        public string UniqueName { get; set; }

        public string FullPath { get; set; }

        public string ProductId { get; set; }

        public Product Product { get; set; }
    }
}
