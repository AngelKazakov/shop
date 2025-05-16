using Microsoft.EntityFrameworkCore;
using RandomShop.Data;

namespace RandomShop.Services.Review;

public class ReviewEligibilityService : IReviewEligibilityService
{
    private readonly ShopContext context;

    public ReviewEligibilityService(ShopContext context)
        => this.context = context;

    public async Task<bool> CanUserLeaveReview(int productId, string userId)
    {
        bool hasPurchased = await context.OrderLines.AnyAsync(ol =>
            ol.ProductItem.ProductId == productId &&
            ol.ShopOrder.UserId == userId &&
            ol.ShopOrder.OrderStatus.Status == "Delivered");

        bool hasLeftReview = await context.UserReviews.AnyAsync(ur =>
            ur.UserId == userId &&
            ur.OrderLine.ProductItem.ProductId == productId);

        return hasPurchased && !hasLeftReview;
    }
}