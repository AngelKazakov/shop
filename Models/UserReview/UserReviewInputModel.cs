using System.ComponentModel.DataAnnotations;
using RandomShop.Data;

namespace RandomShop.Models.UserReview;

public class UserReviewInputModel
{
    [Required]
    public int ProductId { get; set; }  // To know which product the review is for

    [Required]
    public string? UserName { get; set; }  // Optional, as you can fetch this based on the userId

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public double Rating { get; set; }  // The actual rating value

    [Required]
    [StringLength(500, ErrorMessage = "Comment must be between 10 and 500 characters.", MinimumLength = 10)]
    public string Comment { get; set; }  // The review text

    [Required]
    public int? OrderLineId { get; set; }  // To verify that the user purchased this product

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;  // Automatically set when creating
}