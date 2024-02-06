using System.ComponentModel.DataAnnotations.Schema;

namespace RandomShop.Data.Models
{
    public class ProductCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? ParentCategoryId { get; set; }

        public ProductCategory ParentCategory { get; set; }

        //public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

        public ICollection<Variation> Variations { get; set; } = new List<Variation>();

        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
