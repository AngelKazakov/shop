using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public async Task<IActionResult> ViewCart()
    {
        //Check if only user is logged in then create a cart.
        //Now it creates a cart always it doesn't matter if user is logged in...!
        //Check correct applying promotion in shopping cart. Getting the original one not the discounted.
        var userId = this.User.Id();

        var cartItems = await this.cartService.GetCartItemsAsync(userId);

        var model = new CartViewModel() { Items = cartItems };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Add(int id)
    {
        string userId = this.User.Id();

        await this.cartService.AddToCart(userId, id);

        return RedirectToAction("ViewCart");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Remove(int id)
    {
        string userId = this.User.Id();

        await this.cartService.RemoveFromCart(userId, id);

        return RedirectToAction("ViewCart");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Clear()
    {
        string userId = this.User.Id();

        await this.cartService.ClearCart(userId);

        return RedirectToAction("ViewCart");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int id, int quantity)
    {
        string userId = this.User.Id();

        var result = await this.cartService.UpdateQuantityAndGetTotals(userId, id, quantity);

        if (!result.Success)
        {
            return Json(new { success = false, message = result.Message });
        }

        return Json(new
        {
            success = true,
            itemTotal = result.ItemTotal,
            grandTotal = result.GrandTotal
        });
    }
}