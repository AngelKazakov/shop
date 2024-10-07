using Microsoft.AspNetCore.Mvc;
using RandomShop.Models.Variation;
using RandomShop.Services.Variation;

namespace RandomShop.Controllers
{
    public class VariationController : Controller
    {
        private readonly IVariationService variationService;

        public VariationController(IVariationService variationService)
        {
            this.variationService = variationService;
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            VariationAddFormModel? variationAddFormModel = await this.variationService.InitVariationAddFormModel();

            return View(variationAddFormModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(VariationAddFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            VariationViewModel? variationViewModel = await this.variationService.CreateVariation(model);

            return RedirectToAction("Details", new { Id = variationViewModel.Id });
        }
    }
}