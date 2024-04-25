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
            if (await this.CheckIfCategoryAlreadyExists(model.Name))
            {
                throw new DuplicateNameException("Category with given name already exists.");
            }

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
                if (category != null)
                {
                    this.context.Categories.Remove(category);
                    await this.context.SaveChangesAsync();
                }


            }
            catch (Exception ex)
            {
                // Handle exceptions here, including the case where the category is not found
                // Log the exception or perform any necessary cleanup
                // Rethrow the exception or handle it based on your application's requirements
                throw new Exception("Something went wrong. Please try again.", ex);
            }

            return true;
        }

        public async Task<ICollection<CategoryViewModel>> GetAllCategories()
        {
            return await this.context.Categories
                .AsNoTracking()
                .Select(x => new CategoryViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                }).ToListAsync();
        }

        public async Task<ICollection<MainCategoryViewModel>> GetMainCategories()
        {
            List<MainCategoryViewModel> mainCategories = await this.context.Categories
                // .Where(x => x.ParentCategoryId == null)  Where statement is not working properly here...
                .Select(x => new MainCategoryViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                })
                .AsNoTracking()
               .ToListAsync();

            return mainCategories;
        }

        public async Task<ICollection<SubCategoryModel>> GetSubCategories(int parentCategoryId)
        {
            return await this.context.Categories
                .AsNoTracking()
                .Where(x => x.ParentCategoryId == parentCategoryId)
                .Select(x => this.mapper.Map<SubCategoryModel>(x))
                .ToListAsync();
        }

        public async Task<CategoryFormViewModel> InitCategoryFormViewModel()
        {
            ICollection<MainCategoryViewModel> categories = await this.context.Categories.Select(x => new MainCategoryViewModel { Id = x.Id, Name = x.Name, }).ToListAsync();

            CategoryFormViewModel initModel = new CategoryFormViewModel()
            {
                MainCategories = categories,
            };


            return initModel;
        }

        private async Task<bool> CheckIfCategoryAlreadyExists(string categoryName)
        {
            return await this.context.Categories.AsNoTracking().AnyAsync(x => x.Name == categoryName);
        }
    }
}
