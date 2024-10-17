namespace RandomShop.Models.Variation
{
    public class VariationOptionViewModel
    {
        public int VariationId { get; set; }

        public string? VariationName { get; set; }

        public int VariationOptionId { get; set; }

        public ICollection<VariationOptionFormViewModel> VariationOptions { get; set; } = new List<VariationOptionFormViewModel>();
    }
}
