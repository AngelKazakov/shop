using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomShop.Data.Models;
using RandomShop.Exceptions;
using RandomShop.Services.Products;

namespace RandomShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService productService;

        public ProductController(IProductService productService)
        {
            this.productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int productId)
        {
            try
            {
                Product product = await this.productService.GetProductById(productId);
                return View(product);
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

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> GetProducts(string productName)
        {
            var products = await this.productService.GetProductsByGivenName(productName);

            if (!products.Any())
            {
                return View("Home", "Error");
            }

            return View(products);

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
