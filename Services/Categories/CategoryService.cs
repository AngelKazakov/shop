using AutoMapper;
using RandomShop.Data;
using RandomShop.Models.Category;
using System.Data;
using RandomShop.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace RandomShop.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly ShopContext context;
        private readonly IMapper mapper;

        public CategoryService(ShopContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<CategoryViewModel> CreateCategory(CategoryFormModel model)
        {
            Category category = new Category()
            {
                Name = model.Name,
                ParentCategoryId = model.SelectedCategoryFromDropDown ?? null,
            };

            try
            {
                await this.context.Categories.AddAsync(category);
                await this.context.SaveChangesAsync();
            }
            catch (Exception)
            {
                //Implement error handling.
                throw;
            }

            return this.mapper.Map<CategoryViewModel>(category);
        }

        public async Task<bool> DeleteCategory(int id)
        {
            try
            {
                Category? category = await this.context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                this.context.Categories.Remove(category);
                await this.context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }

            return true;
        }

        public async Task<CategoryFormViewModel> InitCategoryFormViewModel()
        {
            List<MainCategoryViewModel> categories = await this.context.Categories.Select(x => new MainCategoryViewModel { Id = x.Id, Name = x.Name, }).ToListAsync();

            CategoryFormViewModel initModel = new CategoryFormViewModel()
            {
                MainCategories = categories,
            };


            return initModel;
        }
    }
}
