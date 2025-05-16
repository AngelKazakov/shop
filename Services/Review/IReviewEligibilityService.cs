namespace RandomShop.Services.Review;

public interface IReviewEligibilityService
{
    public Task<bool> CanUserLeaveReview(int productId, string userId);
}