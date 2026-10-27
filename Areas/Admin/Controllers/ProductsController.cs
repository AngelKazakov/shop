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
        public async Task<IActionResult> Create()
        {
            AdminCreateProductFormModel model = await this.adminProductService.BuildCreateFormAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdminCreateProductFormModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await this.adminProductService.BuildCreateFormAsync(model.CategoryId);
                return View(model);
            }

            //TODO: Implement CreateAsync method in IAdminProductService and AdminProductService to create a new product and return the new product id
            int newProductId = await this.adminProductService.CreateAsync(model);

            if (newProductId <= 0)
            {
                this.logger.LogError("Failed to create a new product.");

                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }

            return RedirectToAction(nameof(Details), new { id = newProductId });
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
                this.logger.LogError(ex, "Failed to load admin product details for product item id {ProductItemId}.",
                    id);

                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            AdminEditProductFormModel? model = await this.adminProductService.GetEditFormAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdminEditProductFormModel model)
        {
            if (!ModelState.IsValid)
            {
                await this.adminProductService.RebuildEditFormAsync(model);
                return View(model);
            }

            bool isProductUpdated = await this.adminProductService.UpdateAsync(model);

            if (!isProductUpdated)
            {
                this.logger.LogError("Failed to update product item with id {ProductItemId}.", model.ProductItemId);

                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }

            return RedirectToAction(nameof(Details), new { id = model.ProductItemId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool isProductDeleted = await this.adminProductService.DeleteAsync(id);

            if (!isProductDeleted)
            {
                this.logger.LogError("Failed to delete product item with id {ProductItemId}.", id);

                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected([FromBody] List<int>? productItemIds)
        {
            if (productItemIds == null || productItemIds.Count == 0)
            {
                return BadRequest(new { message = "No product items were selected for deletion." });
            }

            AdminProductDeleteResult result = await this.adminProductService.DeleteSelectedAsync(productItemIds);

            if (result.Errors.Count > 0)
            {
                return BadRequest(new
                {
                    message = string.Join(" ", result.Errors),
                    requestCount = result.RequestedCount,
                    deletedCount = result.DeletedCount,
                    skippedCount = result.SkippedCount,
                });
            }

            return Ok(new
            {
                message = result.SkippedCount == 0
                    ? $"{result.DeletedCount} product(s) deleted successfully."
                    : $"{result.DeletedCount} product(s) deleted and {result.SkippedCount} skipped.",

                requestCount = result.RequestedCount,
                deletedCount = result.DeletedCount,
                skippedCount = result.SkippedCount,
                deletedIds = result.DeletedIds,
                skippedIds = result.SkippedIds
            });
        }
    }
}