using Microsoft.AspNetCore.Mvc;
using RandomShop.Data.Models;
using RandomShop.Infrastructure;
using RandomShop.Models.Cart;
using RandomShop.Services.Cart;

namespace RandomShop.Controllers;

public class CartController : Controller
{
    private readonly ICartService cartService;

    public CartController(ICartService cartService)
    {
        this.cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> ViewCart()
    {
        var userId = this.User.Id();

        var cartItems = await this.cartService.GetCartItemsAsync(userId);

        var model = new CartViewModel() { Items = cartItems };

        return View(model);
    }
}