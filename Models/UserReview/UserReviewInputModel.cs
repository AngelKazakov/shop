using System.ComponentModel.DataAnnotations;
using RandomShop.Data;

namespace RandomShop.Models.UserReview;

public class UserReviewInputModel
{
    [Required]
    [Range(DataConstants.ratingValueMin, DataConstants.ratingValueMax)]
    public double Rating { get; set; }

    [StringLength(DataConstants.commentMinLength, MinimumLength = DataConstants.commentMaxLength)]
    public string Comment { get; set; }

    [Required]
    public int OrderLineId { get; set; }
}