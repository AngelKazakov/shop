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
}