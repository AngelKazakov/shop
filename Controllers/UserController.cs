using Microsoft.AspNetCore.Mvc;
using RandomShop.Infrastructure;
using RandomShop.Models.Product;
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
        public async Task<IActionResult> RemoveFavorite([FromBody] FavoriteActionRequest request)
        {
            string? userId = this.User.Id();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            bool removed = await userService.RemoveProductFromFavorite(userId, request.ProductId);
            return Ok(new { isFavorite = !removed ? true : false });
        }

        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] FavoriteActionRequest request)
        {
            string? userId = this.User.Id();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            bool added = await userService.AddProductToFavorite(userId, request.ProductId);
            return Ok(new { isFavorite = added });
        }
    }
}