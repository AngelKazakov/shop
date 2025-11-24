using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomShop.Infrastructure;
using RandomShop.Models.Order;
using RandomShop.Services.Order;

namespace RandomShop.Controllers;

public class OrderController : Controller
{
    private readonly IOrderService orderService;

    public OrderController(IOrderService orderService)
    {
        this.orderService = orderService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Checkout()
    {
        string userId = this.User.Id();

        CheckoutViewModel data = await this.orderService.GetCheckoutDataAsync(userId);

        return View(data);
    }

    [HttpGet]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> PlaceOrder(CheckoutFormModel model)
    {
        string userId = this.User.Id();

        if (!ModelState.IsValid)
        {
            var vm = await this.orderService.GetCheckoutDataAsync(userId);
            return View("Checkout", vm);
        }

        var validation = await this.orderService.ValidateCartAsync(userId);
        if (validation.Errors.Any())
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            var vm = await this.orderService.GetCheckoutDataAsync(userId);
            return View("Checkout", vm);
        }

        var orderId = await this.orderService.PlaceOrderAsync(userId, model);

        //Optional confirmation.
        //return RedirectToAction("Confirmation", new { id = orderId });

        return RedirectToAction("Index", "Home");
    }
}