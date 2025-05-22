using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Models.UserReview;

namespace RandomShop.Services.Review;

public class ReviewEligibilityService : IReviewEligibilityService
{
    private readonly ShopContext context;

    public ReviewEligibilityService(ShopContext context)
        => this.context = context;

    public async Task<bool> CanUserLeaveReview(int productId, string userId)
    {
        bool hasPurchased = await CheckIfUserPurchasedProduct(productId, userId);

        bool hasLeftReview = await CheckIfUserAlreadyReviewedProduct(productId, userId);

        return hasPurchased && !hasLeftReview;
    }

    public async Task<EligibleReviewData> GetEligibleOrderLineWithProductDataAsync(int productId, string userId)
    {
        var result = await context.OrderLines
            .Where(ol => ol.ProductItem.ProductId == productId
                         && ol.ShopOrder.UserId == userId
                         && ol.ShopOrder.OrderStatus.Status == "Delivered"
                         && !context.UserReviews.Any(r => r.OrderLineId == ol.Id))
            .Select(ol => new EligibleReviewData
            {
                OrderLineId = ol.Id,
                ProductItemId = ol.ProductItemId,
                ProductId = ol.ProductItem.ProductId
            })
            .FirstOrDefaultAsync();

        return result;
    }

    private async Task<bool> CheckIfUserAlreadyReviewedProduct(int productId, string userId)
    {
        return await this.context.UserReviews
            .AnyAsync(x => x.UserId == userId && x.OrderLine.ProductItem.ProductId == productId);
    }

    private async Task<bool> CheckIfUserPurchasedProduct(int productId, string userId)
    {
        return await this.context.OrderLines
            .AnyAsync(ol => ol.ProductItem.ProductId == productId
                            && ol.ShopOrder.UserId == userId
                            && ol.ShopOrder.OrderStatus.Status == "Delivered");
    }
}