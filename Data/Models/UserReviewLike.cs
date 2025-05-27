using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models;

public class UserReviewLike
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }

    public User User { get; set; }

    [Required]
    public int ReviewId { get; set; }

    public UserReview Review { get; set; }
}