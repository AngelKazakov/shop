using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomShop.Infrastructure;
using RandomShop.Models.Cart;
using RandomShop.Models.Cookie;
using RandomShop.Services.Cart;

namespace RandomShop.Controllers;

public class CartController : Controller
{
    private readonly ICartService cartService;
    private readonly IGuestCartCookieService guestCartCookieService;

    public CartController(ICartService cartService, IGuestCartCookieService guestCartCookieService)
    {
        this.cartService = cartService;
        this.guestCartCookieService = guestCartCookieService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ViewCart()
    {
        //Check if only user is logged in then create a cart.
        //Now it creates a cart always it doesn't matter if user is logged in...!
        //Check correct applying promotion in shopping cart. Getting the original one not the discounted.

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var userId = this.User.Id();
            var cartItems = await this.cartService.GetCartItemsAsync(userId);
            var model = new CartViewModel() { Items = cartItems };

            return View(model);
        }

        var guestItems = this.guestCartCookieService.ReadGuestCart(Request);
        var guestCartItems = await this.guestCartCookieService.GetGuestCart(guestItems);

        return View(guestCartItems);
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
    [AllowAnonymous]
    public IActionResult RemoveFromGuestCart(int id)
    {
        this.guestCartCookieService.RemoveFromGuestCart(Request, Response, id);

        return RedirectToAction("ViewCart");
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Clear()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            string userId = this.User.Id();

            await this.cartService.ClearCart(userId);
        }
        else
        {
            this.guestCartCookieService.ClearGuestCart(Response);
        }


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

    [HttpPost]
    [AllowAnonymous]
    public IActionResult UpdateGuestQuantity(int id, int quantity)
    {
        this.guestCartCookieService.UpdateGuestQuantity(Request, Response, id, quantity);

        return Ok("Quantity updated");
    }

    [HttpGet]
    public async Task<IActionResult> GetCartCount()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            string userId = this.User.Id();
            int count = await this.cartService.GetCartTotalQuantity(userId);

            return Json(new { count });
        }

        var guestItems = this.guestCartCookieService.ReadGuestCart(Request);
        int guestItemCount = guestItems.Sum(x => x.Quantity);

        return Json(new { count = guestItemCount });
    }

    [HttpGet]
    public IActionResult TestWriteCart()
    {
        var items = new List<CartCookieItem>()
        {
            new CartCookieItem() { ProductItemId = 3, Quantity = 1 },
            new CartCookieItem() { ProductItemId = 4, Quantity = 2 },
        };

        this.guestCartCookieService.WriteGuestCart(Response, items);

        return Ok("Cookie written");
    }

    public IActionResult TestReadCart()
    {
        var items = this.guestCartCookieService.ReadGuestCart(Request);

        return Ok("Cookie read:" + items);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AddToGuestCart(int id, int quantity)
    {
        this.guestCartCookieService.AddOrUpdateGuestCart(Request, Response, id, quantity);

        return Ok("Guest cart updated successfully.");
    }
}