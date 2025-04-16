using Microsoft.EntityFrameworkCore;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Models.UserReview;

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

    public async Task<bool> CreateReview(UserReviewInputModel reviewInputModel, string userId)
    {
        int productId = await this.context.OrderLines.Where(x => x.Id == reviewInputModel.OrderLineId)
            .Select(x => x.ProductItem.ProductId).SingleOrDefaultAsync();

        if (productId == 0) return false;

        bool isProductPurchased = await CheckIfUserPurchasedProduct(productId, userId);
        bool isAlreadyReviewed = await CheckIfUserAlreadyReviewedProduct(productId, userId);

        if (!isProductPurchased || isAlreadyReviewed)
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
            UserId = userId,
        };
    }
}