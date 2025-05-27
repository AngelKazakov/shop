namespace RandomShop.Services.Review;

public interface IReviewInteractionService
{
    public Task<bool> ToggleLike(int reviewId, string userId);

    public Task<bool> HasUserLikedReview(int reviewId, string userId);

    public Task<int> GetLikesCountForReview(int reviewId);
}