﻿using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Exceptions;
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

        public async Task<int> CreatePromotion(PromotionAddEditFormModel model)
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

        public async Task<PromotionViewModel> UpdatePromotion(PromotionAddEditFormModel model)
        {
            Promotion? promotionToUpdate = await this.shopContext.Promotions.FindAsync(model.Id);

            if (promotionToUpdate == null) throw new ArgumentNullException(nameof(promotionToUpdate));

            promotionToUpdate.Name = model.Name;
            promotionToUpdate.Description = model.Description;
            promotionToUpdate.DiscountRate = model.DiscountRate;
            promotionToUpdate.StartDate = model.StartDate;
            promotionToUpdate.EndDate = model.EndDate;

            await this.shopContext.SaveChangesAsync();

            return new PromotionViewModel { Id = promotionToUpdate.Id, Name = promotionToUpdate.Name };
        }

        public async Task<bool> DeletePromotion(int id)
        {
            Promotion? promotionForDeletion = await this.shopContext.Promotions.FindAsync(id);

            if (promotionForDeletion == null)
            {
                throw new NotFoundException($"Promotion with ID {id} not found.");
            }

            try
            {
                this.shopContext.Promotions.Remove(promotionForDeletion);
                await this.shopContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                throw new ApplicationException($"An error occurred while deleting the promotion: {ex.Message}");
            }
        }

        public async Task<ICollection<PromotionViewModel>> GetAllPromotions()
        {
            return await this.shopContext.Promotions
            .AsNoTracking()
            .Select(x => new PromotionViewModel { Id = x.Id, Name = x.Name })
            .ToListAsync();
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
                Name = promotion.Name,
                Description = promotion.Description,
                DiscountRate = promotion.DiscountRate,
                EndDate = promotion.EndDate,
                StartDate = promotion.StartDate,
            };

            return promotionViewModel;
        }
    }
}
