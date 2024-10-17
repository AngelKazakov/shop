using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Models.Variation;
using RandomShop.Services.Categories;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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

        public async Task<ICollection<VariationOptionViewModel>> GetVariationOptionBySpecifyCategory(int categoryId)
        {
            List<VariationOptionViewModel> variationOptionViewModels = new List<VariationOptionViewModel>();
            List<Data.Models.Variation> variationOptions;

            if (categoryId == 0)
            {
                variationOptions = await this.shopContext.Variations
                    .AsNoTracking()
                    .Include(x => x.VariationOptions)
                    .AsNoTracking()
                    .ToListAsync();
            }
            else
            {
                variationOptions = await this.shopContext.Variations
                    .AsNoTracking()
                    .Where(x => x.CategoryId == categoryId)
                    .Include(x => x.VariationOptions)
                    .AsNoTracking()
                    .ToListAsync();
            }

            //List<Data.Models.Variation> variations = await this.shopContext.Variations
            //    .AsNoTracking()
            //    .Include(x => x.VariationOptions)
            //    .AsNoTracking()
            //    .ToListAsync();

            foreach (var variation in variationOptions)
            {
                List<VariationOptionFormViewModel> varOptions = new List<VariationOptionFormViewModel>();


                foreach (var variationOption in variation.VariationOptions)
                {
                    varOptions.Add(new VariationOptionFormViewModel() { VariationOptionId = variationOption.Id, Value = variationOption.Value });
                }

                variationOptionViewModels.Add(new VariationOptionViewModel()
                {
                    VariationId = variation.Id,
                    VariationName = variation.Name,
                    VariationOptions = varOptions,
                });

                varOptions = new List<VariationOptionFormViewModel>();
            }


            return variationOptionViewModels;



            //Dictionary<string, List<VariationViewModel>> variation_Options = new Dictionary<string, List<VariationViewModel>>();
            ////Replace key with variation name and values depends on it.
            //List<Data.Models.Variation>? variations = await this.shopContext.Variations.Include(x => x.VariationOptions).AsNoTracking().ToListAsync();

            //foreach (var variation in variations)
            //{
            //    var optionsViewModelList = new List<VariationViewModel>();

            //    foreach (var varOption in variation.VariationOptions)
            //    {
            //        optionsViewModelList.Add(new VariationViewModel()
            //        {
            //            Id = varOption.Id,
            //            Name = variation.Name,
            //            Value = varOption.Value,
            //        });
            //    }

            //    variation_Options.Add(variation.Name, optionsViewModelList);
            //    optionsViewModelList = new List<VariationViewModel>();
            //}

            //return variation_Options;
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
