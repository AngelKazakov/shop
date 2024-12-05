using RandomShop.Models.Variation;

namespace RandomShop.Models.Product
{
    public class ProductDetailsDto
    {
        public decimal Price { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int QuantityInStock { get; set; }
        public string SKU { get; set; }
        public int? CategoryId { get; set; }
        public int? PromotionId { get; set; }

        public List<VariationViewModel> ExistingVariationOptions { get; set; } = new List<VariationViewModel>();

        //Dictionary<string, List<string>> ExistingVariationOptions { get; set; } = new Dictionary<string, List<string>>();
    }
}
