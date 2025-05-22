using Microsoft.AspNetCore.Mvc;
using RandomShop.Infrastructure;
using RandomShop.Models.UserReview;
using RandomShop.Services.Review;

namespace RandomShop.Controllers;

public class ReviewController : Controller
{
    private readonly IUserReviewService userReviewService;
    private readonly IReviewEligibilityService reviewEligibilityService;

    public ReviewController(IUserReviewService userReviewService,
        IReviewEligibilityService reviewEligibilityService)
    {
        this.userReviewService = userReviewService;
        this.reviewEligibilityService = reviewEligibilityService;
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

    [HttpGet]
    public async Task<IActionResult> Create(int productId)
    {
        // string userId = User.Id();
        // var data = await reviewEligibilityService.GetEligibleOrderLineWithProductDataAsync(productId, userId);

        // if (data == null)
        // {
        //     return BadRequest("You cannot leave a review for this product.");
        // }

        // var model = new UserReviewInputModel
        // {
        //     OrderLineId = data.OrderLineId,
        //     ProductId = data.ProductItemId,
        // };

        var model = new UserReviewInputModel
        {
            ProductId = 3,
            Rating = 0,
            Comment = "Awesome product!",
            OrderLineId = 1
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserReviewInputModel model)
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

    [HttpPost]
    public async Task<IActionResult> DeleteMultipleReviews(List<int> reviewIds)
    {
        bool isAdmin = User.IsInRole("Admin");

        if (!isAdmin)
        {
            return Unauthorized("You are not allowed to delete reviews.");
        }

        bool areDeleted = await this.userReviewService.DeleteMultipleReviews(reviewIds, isAdmin);

        if (!areDeleted)
        {
            return NotFound("No reviews found for deletion.");
        }

        return Ok(new { message = "Reviews deleted successfully." });

        //Implement deleting multiple reviews only if the current user is Admin.

        //Add an image and create on date to the review.
        //Implement feature for like/dislike review.
        //User can like review only once other wise when click "Like" should dislike the review.
        //Add likes counting to the model in the database and show it on the review.
        //Make a table if it's needed between user and likes to see which user liked the review so it can be easily managed.
    }
}