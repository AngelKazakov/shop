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

        [HttpGet]
        public async Task<IActionResult> MyFavorites()
        {
            ViewBag.Title = "My Favorite Products";

            var userFavoriteProducts = await this.userService.GetFavoriteProducts(this.User.Id());
            //Fetch favorite products and reuse Product/All View for the favorite products.

            return View("~/Views/Product/All.cshtml", userFavoriteProducts);
        }
    }
}