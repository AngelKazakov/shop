using Microsoft.AspNetCore.Mvc;
using RandomShop.Infrastructure;
using RandomShop.Models.UserReview;
using RandomShop.Services.Review;

namespace RandomShop.Controllers;

public class ReviewController : Controller
{
    private readonly IUserReviewService userReviewService;
    private readonly IReviewEligibilityService reviewEligibilityService;
    private readonly IReviewInteractionService reviewInteractionService;

    public ReviewController(IUserReviewService userReviewService,
        IReviewEligibilityService reviewEligibilityService, IReviewInteractionService reviewInteractionService)
    {
        this.userReviewService = userReviewService;
        this.reviewEligibilityService = reviewEligibilityService;
        this.reviewInteractionService = reviewInteractionService;
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
        string userId = User.Id();
        EligibleReviewData data =
            await reviewEligibilityService.GetEligibleOrderLineWithProductDataAsync(productId, userId);

        if (data == null)
        {
            return BadRequest("You cannot leave a review for this product.");
        }

        var model = new UserReviewInputModel
        {
            OrderLineId = data.OrderLineId,
            ProductId = data.ProductItemId,
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

        TempData["SuccessMessage"] = "Review created successfully!";
        return RedirectToAction("Details", "Product", new { id = model.ProductId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int reviewId)
    {
        string userId = this.User.Id();

        UserReviewInputModel
            model = await this.userReviewService.PopulateReviewInputModel(reviewId, userId);

        if (model == null)
        {
            return NotFound();
        }

        model.ReviewId = reviewId;

        return View("Create", model);
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

        return RedirectToAction("Details", "Product", new { id = model.ProductId });
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
        //Implement feature for like/dislike review.
        //User can like review only once other wise when click "Like" should dislike the review.
        //Add likes counting to the model in the database and show it on the review.
        //Make a table if it's needed between user and likes to see which user liked the review so it can be easily managed.


        //Add an image and create on date to the review.
    }

    [HttpPost]
    public async Task<IActionResult> Like(int reviewId)
    {
        var userId = this.User.Id();

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var isLiked = await this.reviewInteractionService.ToggleLike(reviewId, userId);

        return Ok(new
        {
            liked = isLiked,
            totalLikes = await this.reviewInteractionService.GetLikesCountForReview(reviewId)
        });
    }
}