using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RandomShop.Data.Models
{
    public class ProductCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(DataConstants.productCategoryMaxLength, MinimumLength = DataConstants.productCategoryMinLength)]
        public string Name { get; set; }

        [Required]
        public int? ParentCategoryId { get; set; }

        public ProductCategory ParentCategory { get; set; }

        public ICollection<Variation> Variations { get; set; } = new List<Variation>();

        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
