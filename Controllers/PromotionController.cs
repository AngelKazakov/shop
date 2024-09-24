using Microsoft.AspNetCore.Mvc;
using RandomShop.Models.Promotion;
using RandomShop.Services.Promotions;

namespace RandomShop.Controllers
{
    public class PromotionController : Controller
    {
        private readonly IPromotionService promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            this.promotionService = promotionService;
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(PromotionAddFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int promotionId = await this.promotionService.CreatePromotion(model);

            return RedirectToAction("Details", "PromotionController", promotionId);
        }
    }
}
