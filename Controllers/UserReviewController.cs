using Microsoft.AspNetCore.Mvc;
using RandomShop.Infrastructure;
using RandomShop.Models.UserReview;
using RandomShop.Services.Review;

namespace RandomShop.Controllers;

public class UserReviewController : Controller
{
    private readonly IUserReviewService userReviewService;

    public UserReviewController(IUserReviewService userReviewService)
    {
        this.userReviewService = userReviewService;
    }

    [HttpPost]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        string? userId = this.User.Id();

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        bool isReviewDelete = await this.userReviewService.DeleteReview(reviewId, userId);

        if (!isReviewDelete)
        {
            return NotFound();
        }

        return Ok(new { message = "Review deleted successfully." });
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview(UserReviewInputModel model)
    {
        string? userId = this.User.Id();

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        bool isReviewCreated = await this.userReviewService.CreateReview(model, userId);

        if (!isReviewCreated)
        {
            return BadRequest("You are not allowed to review this product or you already submitted a review.");
        }

        return Ok(new { message = "Review created successfully." });
    }

    [HttpPost]
    public async Task<IActionResult> EditReview(UserReviewInputModel model, int reviewId)
    {
        string? userId = this.User.Id();

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        bool isSuccessfullyEdited = await this.userReviewService.EditReview(model, reviewId, userId);

        if (!isSuccessfullyEdited)
        {
            return BadRequest("You are not allowed to edit this review.");
        }

        return Ok(new { message = "Review edited successfully." });
    }
}