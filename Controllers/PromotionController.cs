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

            int createdPromotionId = await this.promotionService.CreatePromotion(model);

            return RedirectToAction("Details", "Promotion", new { Id = createdPromotionId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int Id)
        {
            PromotionViewModel? model = await this.promotionService.GetPromotionById(Id);

            if (model == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            return View(await this.promotionService.GetAllPromotions());
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int Id)
        {
            bool isDeleted = await this.promotionService.DeletePromotion(Id);

            if (!isDeleted)
            {
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
