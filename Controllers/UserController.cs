using Microsoft.AspNetCore.Mvc;
using RandomShop.Infrastructure;
using RandomShop.Services.User;

namespace RandomShop.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> AddFavorite(int productId)
        {
            //Check if this type of getting current user Id is working properly.
            string? userId = this.User.Id();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            bool isAddedToFavorite = await userService.AddProductToFavorite(userId, productId);

            return Ok(new { isFavorite = isAddedToFavorite });
        }
    }
}
