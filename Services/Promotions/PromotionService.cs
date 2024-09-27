using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Models.Promotion;

namespace RandomShop.Services.Promotions
{
    public class PromotionService : IPromotionService
    {
        private readonly ShopContext shopContext;

        public PromotionService(ShopContext shopContext)
        {
            this.shopContext = shopContext;
        }

        public async Task<int> CreatePromotion(PromotionAddFormModel model)
        {
            Promotion? promotion = new Promotion();

            if (model is not null)
            {
                promotion.Name = model.Name;
                promotion.Description = model.Description;
                promotion.DiscountRate = model.DiscountRate;
                promotion.StartDate = model.StartDate;
                promotion.EndDate = model.EndDate;

                await this.shopContext.Promotions.AddAsync(promotion);
                await this.shopContext.SaveChangesAsync();
            }

            return promotion.Id;
        }

        public async Task<PromotionViewModel> GetPromotionById(int id)
        {
            Promotion? promotion = await this.shopContext.Promotions.FindAsync(id);

            if (promotion == null)
            {
                return null;
            }

            PromotionViewModel promotionViewModel = new PromotionViewModel
            {
                Id = promotion.Id,
                Name = promotion.Name
            };

            return promotionViewModel;
        }
    }
}
