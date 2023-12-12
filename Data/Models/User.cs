using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;

namespace RandomShop.Data.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();

        public ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();

        public ICollection<UserReview> UserReviews { get; set; } = new List<UserReview>();

        public ICollection<UserPaymentMethod> UserPaymentMethods { get; set; } = new List<UserPaymentMethod>();
    }
}
