using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Models.UserReview;

namespace RandomShop.Services.Review;

public class UserReviewService : IUserReviewService
{
    private readonly ShopContext context;
    private readonly IReviewEligibilityService reviewEligibilityService;

    public UserReviewService(ShopContext context, IReviewEligibilityService reviewEligibilityService)
    {
        this.context = context;
        this.reviewEligibilityService = reviewEligibilityService;
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

    public async Task<bool> DeleteMultipleReviews(List<int> reviewIds, bool isAdmin = false)
    {
        if (!isAdmin)
        {
            throw new UnauthorizedAccessException("Only admins can delete multiple reviews.");
        }

        var reviewsToDelete = await this.context.UserReviews
            .Where(x => reviewIds.Contains(x.Id))
            .ToListAsync();

        if (!reviewsToDelete.Any())
        {
            return false;
        }

        this.context.UserReviews.RemoveRange(reviewsToDelete);
        await this.context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateReview(UserReviewInputModel reviewInputModel, string userId)
    {
        int productId = await this.context.OrderLines.Where(x => x.Id == reviewInputModel.OrderLineId)
            .Select(x => x.ProductItem.ProductId).SingleOrDefaultAsync();

        if (productId == 0) return false;

        if (!await this.reviewEligibilityService.CanUserLeaveReview(productId, userId))
        {
            return false;
        }

        try
        {
            UserReview userReview = MapToUserReview(reviewInputModel, userId);

            await this.context.UserReviews.AddAsync(userReview);
            await this.context.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> EditReview(UserReviewInputModel reviewInputModel, int reviewId, string userId)
    {
        UserReview? reviewForEdit =
            await this.context.UserReviews.FirstOrDefaultAsync(x => x.Id == reviewId && x.UserId == userId);

        if (reviewForEdit == null)
        {
            throw new UnauthorizedAccessException("Cannot edit this review.");
        }

        try
        {
            reviewForEdit.RatingValue = reviewInputModel.Rating;
            reviewForEdit.Comment = reviewInputModel.Comment;

            await this.context.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // public async Task<bool> CanUserLeaveReview(int productId, string userId)
    // {
    //     bool hasPurchased = await CheckIfUserPurchasedProduct(productId, userId);
    //     bool hasLeftReview = await CheckIfUserAlreadyReviewedProduct(productId, userId);

    //     if (hasPurchased && !hasLeftReview)
    //     {
    //         return true;
    //     }

    //     return false;
    // }

    // private Task<bool> IsAllowedToEdit(int reviewId, string userId)
    // {
    //     return this.context.UserReviews.AnyAsync(x => x.Id == reviewId && x.UserId == userId);
    // }

    private async Task<bool> CheckIfUserAlreadyReviewedProduct(int productId, string userId)
    {
        bool isAlreadyReviewed =
            await context.UserReviews.AnyAsync(x =>
                x.UserId == userId && x.OrderLine.ProductItem.ProductId == productId);

        return isAlreadyReviewed;
    }

    private async Task<bool> CheckIfUserPurchasedProduct(int productId, string userId)
    {
        return await context.OrderLines.AnyAsync(ol =>
            ol.ShopOrder.UserId == userId &&
            ol.ProductItem.ProductId == productId &&
            ol.ShopOrder.OrderStatus.Status == "Delivered");
    }

    private UserReview MapToUserReview(UserReviewInputModel reviewInputModel, string userId)
    {
        return new UserReview()
        {
            OrderLineId = reviewInputModel.OrderLineId,
            RatingValue = reviewInputModel.Rating,
            Comment = reviewInputModel.Comment,
            CreatedOn = DateTime.Now,
            UserId = userId,
        };
    }
}