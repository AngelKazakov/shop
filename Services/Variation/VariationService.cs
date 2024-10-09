using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;
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
            CheckIfModelIsNullAndThrowArgumentNullException(model);

            try
            {
                Data.Models.Variation? newVariation = new Data.Models.Variation()
                {
                    CategoryId = model.CategoryId,
                    Name = model.Name,
                };


                await this.shopContext.Variations.AddAsync(newVariation);
                await this.shopContext.SaveChangesAsync();

                VariationViewModel? newAddedVariation = await CreateVariationOption(newVariation.Id, model.Value);
                newAddedVariation.Name = newVariation.Name;

                return newAddedVariation; //new VariationViewModel() { Id = newAddedVariation.Id, Name = newAddedVariation.Name, Value = newAddedVariation.Value };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating a variation", ex);
            }
        }

        public async Task<VariationViewModel> CreateVariationOption(int variationId, string variationValue)
        {
            CheckIfModelIsNullAndThrowArgumentNullException(new { VariationId = variationId, VariationValue = variationValue });

            try
            {
                VariationOption newVarOption = new VariationOption()
                {
                    VariationId = variationId,
                    Value = variationValue,
                };

                await this.shopContext.VariationOptions.AddAsync(newVarOption);
                await this.shopContext.SaveChangesAsync();

                return new VariationViewModel() { Id = newVarOption.Id, Value = newVarOption.Value };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating a variation option", ex);
            }
        }

        public async Task<bool> AddValueToVariationOption(VariationOptionAddFormModel model)
        {
            CheckIfModelIsNullAndThrowArgumentNullException(model);

            try
            {
                VariationOption? existingVarOptionWithSameName = await this.shopContext.VariationOptions
                    .FirstOrDefaultAsync(x => x.Value == model.Value && x.VariationId == model.VariationId);

                if (existingVarOptionWithSameName is null)
                {
                    VariationOption? varOption = new VariationOption() { Value = model.Value, VariationId = model.VariationId };
                    await this.shopContext.VariationOptions.AddAsync(varOption);
                    await this.shopContext.SaveChangesAsync();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error adding value on variation", ex);
            }
        }

        public async Task<VariationAddFormModel> InitVariationAddFormModel()
        {
            return new VariationAddFormModel()
            {
                Categories = await this.categoryService.GetAllCategories(),
            };
        }

        public async Task<VariationOptionAddFormModel> InitVariationOptionAddFormModel()
        {
            List<VariationViewModel>? variationsViewModels = await this.shopContext.Variations
                .AsNoTracking().Select(x => new VariationViewModel() { Id = x.Id, Name = x.Name }).ToListAsync();

            return new VariationOptionAddFormModel()
            {
                Variations = variationsViewModels,
            };
        }

        private void CheckIfModelIsNullAndThrowArgumentNullException(object model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }
        }
    }
}
