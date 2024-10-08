using RandomShop.Models.Variation;

namespace RandomShop.Services.Variation
{
    public interface IVariationService
    {
        public Task<VariationAddFormModel> InitVariationAddFormModel();

        public Task<VariationViewModel> CreateVariation(VariationAddFormModel model);

        public Task<VariationViewModel> CreateVariationOption(int variationId, string variationValue);
    }
}
