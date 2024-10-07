using RandomShop.Data;
using RandomShop.Models.Variation;
using RandomShop.Services.Categories;

namespace RandomShop.Services.Variation
{
    public class VariationService : IVariationService
    {
        private readonly ShopContext shopContext;
        private readonly ICategoryService categoryService;

        public VariationService(ShopContext shopContext, ICategoryService categoryService)
        {
            this.shopContext = shopContext;
            this.categoryService = categoryService;
        }

        public async Task<VariationViewModel> CreateVariation(VariationAddFormModel model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                Data.Models.Variation? newVariation = new Data.Models.Variation()
                {
                    CategoryId = model.CategoryId,
                    Name = model.Name,
                };


                await this.shopContext.Variations.AddAsync(newVariation);
                await this.shopContext.SaveChangesAsync();

                return new VariationViewModel() { Id = newVariation.Id, Name = newVariation.Name };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating a variation", ex);
            }
        }

        public async Task<VariationAddFormModel> InitVariationAddFormModel()
        {
            return new VariationAddFormModel()
            {
                Categories = await this.categoryService.GetAllCategories(),
            };
        }
    }
}
