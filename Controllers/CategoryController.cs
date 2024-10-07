using Microsoft.AspNetCore.Mvc;
using RandomShop.Data.Models;
using RandomShop.Models.Category;
using RandomShop.Services.Categories;

namespace RandomShop.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            this.categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            return View(await this.categoryService.InitCategoryFormViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Add(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await this.categoryService.CreateCategory(model.CategoryFormModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "HomeController", ex);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {

            return View(await this.categoryService.InitUpdateCategoryModel());
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateCategoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);

            }

            bool isCategoryUpdatedSuccessfully = await this.categoryService.UpdateCategory(model);

            if (!isCategoryUpdatedSuccessfully)
            {
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int categoryId)
        {
            bool statement = await this.categoryService.DeleteCategory(categoryId);

            if (!statement)
            {
                return RedirectToAction("Error", "Home");

            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<ICollection<CategoryViewModel>> GetCategories()
        {
            var categories = await this.categoryService.GetAllCategories();

            return categories;
        }
    }
}
