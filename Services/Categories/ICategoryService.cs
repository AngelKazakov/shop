using RandomShop.Models.Category;

namespace RandomShop.Services.Categories
{
    public interface ICategoryService
    {
        public Task<CategoryViewModel> CreateCategory(CategoryFormModel model);

        public Task<bool> UpdateCategory(UpdateCategoryModel model);

        public Task<bool> DeleteCategory(int id);

        public Task<CategoryFormViewModel> InitCategoryFormViewModel();

        public Task<UpdateCategoryModel> InitUpdateCategoryModel();

        public Task<CategoryViewModel> GetCategory(int id);

        public Task<ICollection<CategoryViewModel>> GetAllCategories();

        public Task<ICollection<MainCategoryViewModel>> GetMainCategories();

        public Task<ICollection<SubCategoryModel>> GetSubCategories(int parentCategoryId);
    }
}
