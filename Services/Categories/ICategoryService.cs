using RandomShop.Models.Category;

namespace RandomShop.Services.Categories
{
    public interface ICategoryService
    {
        public Task<CategoryViewModel> CreateCategory(CategoryFormModel model);

        public Task<bool> DeleteCategory(int id);

        public Task<CategoryFormViewModel> InitCategoryFormViewModel();
    }
}
