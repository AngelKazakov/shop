using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;

namespace RandomShop.Data.Models
{
    public class User : IdentityUser
    {
        //[Required]
        //[StringLength(DataConstants.firstAndLastNameMaxLength, MinimumLength = DataConstants.firstAndLastNameMinLength)]
        public string? FirstName { get; set; }

        //[Required]
        //[StringLength(DataConstants.firstAndLastNameMaxLength, MinimumLength = DataConstants.firstAndLastNameMinLength)]
        public string? LastName { get; set; }

        //[Required]
        public int? ShoppingCartId { get; set; }

        public ShoppingCart ShoppingCart { get; set; }

        public ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();

        public ICollection<UserReview> UserReviews { get; set; } = new List<UserReview>();

        public ICollection<UserPaymentMethod> UserPaymentMethods { get; set; } = new List<UserPaymentMethod>();

        public ICollection<UserFavoriteProduct> UserFavoriteProducts { get; set; } = new HashSet<UserFavoriteProduct>();
    }
}
