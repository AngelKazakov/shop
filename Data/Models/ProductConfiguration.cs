using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class ProductConfiguration
    {
        public int ProductItemId { get; set; }

        public ProductItem ProductItem { get; set; }

        public int VariationOptionId { get; set; }

        public VariationOption VariationOption { get; set; }
    }
}
