using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomShop.Models.Product;
using RandomShop.Models.Variation;
using RandomShop.Services.Products;
using RandomShop.Services.Variation;

namespace RandomShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService productService;
        private readonly IVariationService variationService;
        public ProductController(IProductService productService, IVariationService variationService)
        {
            this.productService = productService;
            this.variationService = variationService;
        }

        [HttpGet]
        public async Task<IActionResult> Add(int categoryId)
        {
            return View(await this.productService.InitProductAddFormModel(categoryId));
        }

        [HttpPost]
        public async Task<IActionResult> Add(ProductAddFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int addedProductId = await this.productService.AddProduct(model);

            return RedirectToAction("Details", new { id = addedProductId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ProductEditFormModel? model = await this.productService.InitProductEditFormModel(id);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductEditFormModel model)
        {
            // Split => model.ImagesForDelete so the id's on images which are for deletion can be accessed.
            int editedProductId = await this.productService.EditProduct(model);

            if (editedProductId != null && editedProductId > 0)
            {
                return RedirectToAction("Details", new { id = editedProductId });
            }

            return RedirectToAction("Error", "Home");
        }

        [HttpGet]
        public async Task<JsonResult> GetVariationOptionsByCategory(int categoryId)
        {
            List<VariationOptionViewModel> variations = await this.variationService.GetVariationOptionBySpecifyCategory(categoryId);
            return Json(variations);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            ProductViewModel productDetails = await this.productService.GetProductById(id);

            if (productDetails is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(productDetails);
        }


        [HttpGet]
        public async Task<IActionResult> Search(string productName)
        {
            ICollection<ProductListViewModel> foundProducts = await this.productService.GetProductsByName(productName);

            return View("All", foundProducts);
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            ICollection<ProductListViewModel> productList = await this.productService.GetAllProducts();

            return View(productList);
        }

        [HttpGet]
        [Route("Product/GetByCategory/{categoryId:int}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            try
            {
                var productsByCategoryId = await this.productService.GetProductsByCategory(categoryId);
                return View("All", productsByCategoryId);
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        [Route("Product/GetByPromotion/{promotionId:int}")]
        public async Task<IActionResult> GetByPromotion(int promotionId)
        {
            try
            {
                var productsByPromotion = await this.productService.GetProductsByPromotion(promotionId);
                return View("All", productsByPromotion);
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Compare(List<int> productIds)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            bool isProductDeleted = await this.productService.DeleteProduct(id);

            if (isProductDeleted)
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Error", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSelected([FromBody] List<int> selectedProductIds)
        {
            if (selectedProductIds != null && selectedProductIds.Any())
            {
                await productService.DeleteSelectedProducts(selectedProductIds);
                return RedirectToAction("All");
            }

            return BadRequest("No products selected for deletion.");
        }
    }
}
