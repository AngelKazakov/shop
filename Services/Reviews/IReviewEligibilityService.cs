using RandomShop.Models.UserReview;

namespace RandomShop.Services.Review;

public interface IReviewEligibilityService
{
    public Task<bool> CanUserLeaveReview(int productId, string userId);

    public Task<EligibleReviewData> GetEligibleOrderLineWithProductDataAsync(int productId, string userId);
}