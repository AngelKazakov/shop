using RandomShop.Data.Models;
using RandomShop.Models.UserReview;

namespace RandomShop.Services.Review;

public interface IUserReviewService
{
    public Task<bool> DeleteReview(int reviewId, string userId);

    public Task<bool> DeleteMultipleReviews(List<int> reviewIds, bool isAdmin = false);

    public Task<bool> CreateReview(UserReviewInputModel reviewInputModel, string userId);

    public Task<bool> EditReview(UserReviewInputModel reviewInputModel, int reviewId, string userId);

    //  public Task<bool> CanUserLeaveReview(int productId, string userId);
}