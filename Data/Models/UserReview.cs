using System.ComponentModel.DataAnnotations;

namespace RandomShop.Data.Models
{
    public class UserReview
    {
        public int Id { get; set; }

        [Required] public string UserId { get; set; }

        public User User { get; set; }

        [Required]
        [Range(DataConstants.ratingValueMin, DataConstants.ratingValueMax)]
        public double RatingValue { get; set; }

        [Range(DataConstants.commentMinLength, DataConstants.commentMaxLength)]
        public string Comment { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required] public int OrderLineId { get; set; }

        public OrderLine OrderLine { get; set; }
    }
}