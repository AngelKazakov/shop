using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomShop.Infrastructure;
using RandomShop.Models.Product;
using RandomShop.Services.Cart;
using RandomShop.Services.User;

namespace RandomShop.Controllers;

public class FavoriteController : Controller
{
    private readonly IUserService userService;
    private readonly IGuestFavoritesCookieService guestFavoritesCookieService;

    public FavoriteController(IUserService userService, IGuestFavoritesCookieService guestFavoritesCookieService)
    {
        this.userService = userService;
        this.guestFavoritesCookieService = guestFavoritesCookieService;
    }


    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ToggleFavorite([FromBody] FavoriteActionRequest request)
    {
        bool isFavorite;

        if (User.Identity?.IsAuthenticated == true)
        {
            string userId = this.User.Id();
            isFavorite = await this.userService.ToggleFavoriteAsync(userId, request.ProductId);
        }
        else
        {
            isFavorite = guestFavoritesCookieService.ToggleFavorite(Request, Response, request.ProductId);
        }

        return Ok(new { isFavorite });
    }
}