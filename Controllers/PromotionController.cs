using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RandomShop.Models.Promotion;
using RandomShop.Services.Promotions;

namespace RandomShop.Controllers
{
    public class PromotionController : Controller
    {
        private readonly IPromotionService promotionService;
        private readonly IMapper mapper;
        public PromotionController(IPromotionService promotionService, IMapper mapper)
        {
            this.promotionService = promotionService;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var model = new PromotionAddEditFormModel(); // Initialize with a new model for adding
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(PromotionAddEditFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int createdPromotionId = await this.promotionService.CreatePromotion(model);

            return RedirectToAction("Details", "Promotion", new { Id = createdPromotionId });
        }

        [HttpGet]
        public async Task<IActionResult> Update(int Id)
        {
            PromotionViewModel? promotionForUpdate = await this.promotionService.GetPromotionById(Id);

            //Fill the input fields with correct data.
            var model = this.mapper.Map<PromotionAddEditFormModel>(promotionForUpdate);

            return View("Add", model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(PromotionAddEditFormModel promotionAddEditFormModel)
        {
            if (!ModelState.IsValid)
            {
                return View(promotionAddEditFormModel);
            }

            var updatedPromotion = await this.promotionService.UpdatePromotion(promotionAddEditFormModel);

            return RedirectToAction("Details", "Promotion", new { Id = updatedPromotion.Id });
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
    }
}
