using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomShop.Infrastructure;
using RandomShop.Models.Cart;
using RandomShop.Models.Order;
using RandomShop.Services.Address;
using RandomShop.Services.Cart;
using RandomShop.Services.Order;

namespace RandomShop.Controllers;

public class OrderController : Controller
{
    private readonly IOrderService orderService;
    private readonly IAddressService addressService;
    private readonly IMapper mapper;

    public OrderController(IOrderService orderService, IMapper mapper, IAddressService addressService)
    {
        this.orderService = orderService;
        this.mapper = mapper;
        this.addressService = addressService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Checkout()
    {
        string userId = this.User.Id();

        CheckoutViewModel data = await this.orderService.GetCheckoutDataAsync(userId);

        return View(data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> PlaceOrder(CheckoutFormModel model)
    {
        string userId = this.User.Id();

        CheckoutViewModel viewModel;

        if (!ModelState.IsValid)
        {
            viewModel = await this.orderService.GetCheckoutDataAsync(userId, model.SelectedShippingMethodId);
            this.mapper.Map(model, viewModel);
            return View("Checkout", viewModel);
        }

        Dictionary<string, string> addressErrors =
            this.addressService.ValidateAddressSelection(model.Address, model.UseNewAddress,
                model.SelectedAddressId);

        if (addressErrors.Any())
        {
            foreach (var kvp in addressErrors)
            {
                ModelState.AddModelError(kvp.Key, kvp.Value);
            }

            viewModel = await this.orderService.GetCheckoutDataAsync(userId, model.SelectedShippingMethodId);
            this.mapper.Map(model, viewModel);

            return View("Checkout", viewModel);
        }

        CartValidationResult validation = await this.orderService.ValidateCartAsync(userId);

        if (validation.Errors.Any())
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            viewModel = await this.orderService.GetCheckoutDataAsync(userId, model.SelectedShippingMethodId);
            this.mapper.Map(model, viewModel);

            return View("Checkout", viewModel);
        }

        int orderId = await this.orderService.PlaceOrderAsync(userId, model);

        //Optional confirmation.
        //return RedirectToAction("Confirmation", new { id = orderId });

        return RedirectToAction(nameof(Confirmation), new { orderId = orderId });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Confirmation(int orderId)
    {
        string userId = this.User.Id();

        OrderConfirmationViewModel? model = await this.orderService.GetOrderDetailsAsync(orderId, userId);

        if (model == null)
        {
            return NotFound(); //Or make custom error view and return it.
        }

        return View(model);
    }
}
