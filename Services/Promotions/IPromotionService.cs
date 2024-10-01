using RandomShop.Models.Promotion;

namespace RandomShop.Services.Promotions
{
    public interface IPromotionService
    {
        public Task<int> CreatePromotion(PromotionAddEditFormModel model);

        public Task<bool> DeletePromotion(int id);

        public Task<PromotionViewModel> UpdatePromotion(PromotionAddEditFormModel model);

        public Task<PromotionViewModel> GetPromotionById(int id);

        public Task<ICollection<PromotionViewModel>> GetAllPromotions();

    }
}
