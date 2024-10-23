using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RandomShop.Data.Models;
using RandomShop.Exceptions;
using RandomShop.Models.Product;
using RandomShop.Services.Products;
using Newtonsoft.Json;
using NuGet.Protocol;
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

            return Redirect($"Product/Get/{addedProductId}");
        }

        [HttpGet]
        public async Task<JsonResult> GetVariationOptionsByCategory(int categoryId)
        {
            var variations = await this.variationService.GetVariationOptionBySpecifyCategory(categoryId);
            return Json(variations);
        }

        [HttpGet]
        public async Task<IActionResult> Get(int productId)
        {
            try
            {
                Product product = await this.productService.GetProductById(productId);
                // return View(product);

                return null;
            }
            catch (NotFoundException ex)
            {
                //Log the exception...
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "An unexpected error occurred.";
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Details(string productName)
        {
            var product = await this.productService.GetProductByName(productName);

            if (product == null)
            {
                return View("Error");
            }

            //return View(product);
            return null;
        }

        [HttpPost]
        public async Task<IActionResult> SearchProducts(string productName)
        {
            var products = await this.productService.GetProductsByName(productName);

            if (!products.Any())
            {
                return RedirectToAction("Error", "Home");
            }

            //return View(products);

            return null;

        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            ICollection<ProductListViewModel> productList = await this.productService.GetAllProducts();

            return View(productList);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int productId)
        {
            bool isProductDeleted = await this.productService.DeleteProduct(productId);

            if (isProductDeleted)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
