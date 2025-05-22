using System.ComponentModel.DataAnnotations;
using RandomShop.Data;

namespace RandomShop.Models.UserReview;

public class UserReviewInputModel
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public double Rating { get; set; }

    [Required]
    [StringLength(500, ErrorMessage = "Comment must be between 10 and 500 characters.", MinimumLength = 10)]
    public string Comment { get; set; }

    [Required]
    public int OrderLineId { get; set; } // Make it non-nullable
}