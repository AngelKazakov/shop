using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class Product
    {
        public int Id { get; init; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int ProductCategoryId { get; set; }

        public ProductCategory ProductCategory { get; set; }

        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

        public ICollection<ProductItem> ProductItems { get; set; } = new List<ProductItem>();
    }
}
