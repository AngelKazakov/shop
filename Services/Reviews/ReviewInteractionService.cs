using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;

namespace RandomShop.Services.Review;

public class ReviewInteractionService : IReviewInteractionService
{
    private readonly ShopContext context;

    public ReviewInteractionService(ShopContext context)
    {
        this.context = context;
    }

    public async Task<bool> ToggleLike(int reviewId, string userId)
    {
        var existingLike =
            await this.context.ReviewLikes.FirstOrDefaultAsync(x => x.ReviewId == reviewId && x.UserId == userId);

        if (existingLike == null)
        {
            UserReviewLike likeReview = new UserReviewLike()
            {
                ReviewId = reviewId,
                UserId = userId,
            };

            await this.context.ReviewLikes.AddAsync(likeReview);
            await this.context.SaveChangesAsync();

            return true;
        }
        else
        {
            this.context.ReviewLikes.Remove(existingLike);
            await this.context.SaveChangesAsync();

            return false;
        }
    }

    public async Task<bool> HasUserLikedReview(int reviewId, string userId)
    {
        return await this.context.ReviewLikes.AnyAsync(x => x.ReviewId == reviewId && x.UserId == userId);
    }

    public async Task<int> GetLikesCountForReview(int reviewId)
    {
        return await this.context.ReviewLikes.CountAsync(x => x.ReviewId == reviewId);
    }
}