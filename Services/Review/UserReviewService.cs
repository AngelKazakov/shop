using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;

namespace RandomShop.Services.Review;

public class UserReviewService : IUserReviewService
{
    private readonly ShopContext context;

    public UserReviewService(ShopContext context)
    {
        this.context = context;
    }

    public async Task<bool> DeleteReview(int reviewId, string userId)
    {
        UserReview? reviewForDeletion = await this.context.UserReviews.FirstOrDefaultAsync(x => x.Id == reviewId);

        if (reviewForDeletion != null && reviewForDeletion.UserId == userId)
        {
            this.context.UserReviews.Remove(reviewForDeletion);
            await this.context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}