using RandomShop.Models.Promotion;

namespace RandomShop.Services.Promotions
{
    public interface IPromotionService
    {
        public Task<int> CreatePromotion(PromotionAddFormModel model);

        public Task<PromotionViewModel> GetPromotionById(int id);

        public Task<ICollection<PromotionViewModel>> GetAllPromotions();
    }
}
