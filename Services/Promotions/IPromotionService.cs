using RandomShop.Models.Promotion;

namespace RandomShop.Services.Promotions
{
    public interface IPromotionService
    {
        public Task<int> CreatePromotion(PromotionAddFormModel model);
    }
}
