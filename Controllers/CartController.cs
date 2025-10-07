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
        //Try not to redirect or reload the whole page, just to update the number of quantity per product...
        string userId = this.User.Id();

        await this.cartService.UpdateQuantity(userId, id, quantity);

        var cartItems = await this.cartService.GetCartItemsAsync(userId);
        var updatedItems = cartItems.FirstOrDefault(ci => ci.ProductItemId == id);

        if (updatedItems == null)
        {
            return Json(new { success = false, message = "Item not found" });
        }

        var response = new
        {
            success = true,
            itemTotal = updatedItems.TotalPrice,
            grandTotal = cartItems.Sum(ci => ci.TotalPrice),
        };


        return Json(response);
    }
}