using Microsoft.AspNetCore.Mvc;
using RandomShop.Models.Category;
using RandomShop.Services.Categories;

namespace RandomShop.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService categoryService;

        public CategoryController(ICategoryService categoryService)
         => this.categoryService = categoryService;

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
    }
}
