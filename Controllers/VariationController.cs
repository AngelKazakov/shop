using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        [HttpGet]
        public async Task<IActionResult> AddValue()
        {
            return View(await this.variationService.InitVariationOptionAddFormModel());
        }

        [HttpPost]
        public async Task<IActionResult> AddValue(VariationOptionAddFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool result = await this.variationService.AddValueToVariationOption(model);

            if (!result)
            {
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}