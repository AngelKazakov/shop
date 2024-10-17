using RandomShop.Models.Variation;

namespace RandomShop.Services.Variation
{
    public interface IVariationService
    {
        public Task<VariationAddFormModel> InitVariationAddFormModel();

        public Task<VariationOptionAddFormModel> InitVariationOptionAddFormModel();

        public Task<VariationViewModel> CreateVariation(VariationAddFormModel model);

        public Task<VariationViewModel> CreateVariationOption(int variationId, string variationValue);

        public Task<bool> AddValueToVariationOption(VariationOptionAddFormModel model);

        public Task<ICollection<VariationOptionViewModel>> GetVariationOptionBySpecifyCategory(int categoryId);
    }
}
