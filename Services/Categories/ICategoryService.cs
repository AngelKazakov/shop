using RandomShop.Models.Category;

namespace RandomShop.Services.Categories
{
    public interface ICategoryService
    {
        public Task<CategoryViewModel> CreateCategory(CategoryFormModel model);

        public Task<CategoryFormViewModel> InitCategoryFormViewModel();
    }
}
