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
    }
}
