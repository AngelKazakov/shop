using RandomShop.Data.Models;
using RandomShop.Models.UserReview;

namespace RandomShop.Services.Review;

public interface IUserReviewService
{
    public Task<bool> DeleteReview(int reviewId);
}