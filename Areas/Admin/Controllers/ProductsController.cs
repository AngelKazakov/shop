using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomShop.Areas.Admin.Models;
using RandomShop.Areas.Admin.Services.Product;
using RandomShop.Models;
using System.Diagnostics;

namespace RandomShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly IAdminProductService adminProductService;
        private readonly ILogger<ProductsController> logger;

        public ProductsController(IAdminProductService adminProductService, ILogger<ProductsController> logger)
        {
            this.adminProductService = adminProductService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] AdminProductListQueryModel query)
        {
            try
            {
                AdminProductListQueryModel result =
                    await this.adminProductService.GetPagedAsync(query);

                return View(result);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to load admin products list.");

                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                AdminProductDetailsViewModel? model = await this.adminProductService.GetDetailsAsync(id);

                if (model == null)
                {
                    return NotFound();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to load admin product details for product item id {ProductItemId}.", id);

                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
        }
    }
}
