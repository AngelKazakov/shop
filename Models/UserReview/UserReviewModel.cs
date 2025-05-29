namespace RandomShop.Models.UserReview;

public class UserReviewModel
{
    public int ReviewId { get; set; }

    public string UserName { get; set; }

    public double RatingValue { get; set; }

    public string Comment { get; set; }

    public DateTime CreatedOn { get; set; }

    public int TotalLikes { get; set; }

    public bool IsLikedByCurrentUser { get; set; }
}