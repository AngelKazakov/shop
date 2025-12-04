using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RandomShop.Infrastructure;
using RandomShop.Models.Cart;
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> PlaceOrder(CheckoutFormModel model)
    {
        string userId = this.User.Id();

        CheckoutViewModel viewModel;

        if (!ModelState.IsValid)
        {
            viewModel = await this.orderService.GetCheckoutDataAsync(userId);

            RepopulateViewModel(viewModel, model);

            return View("Checkout", viewModel);
        }

        var addressErrors = this.orderService.ValidateAddressSelection(model);

        if (addressErrors.Any())
        {
            foreach (var kvp in addressErrors)
            {
                ModelState.AddModelError(kvp.Key, kvp.Value);
            }

            viewModel = await this.orderService.GetCheckoutDataAsync(userId);
            RepopulateViewModel(viewModel, model);

            return View("Checkout", viewModel);
        }

        CartValidationResult validation = await this.orderService.ValidateCartAsync(userId);

        if (validation.Errors.Any())
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            viewModel = await this.orderService.GetCheckoutDataAsync(userId);
            RepopulateViewModel(viewModel, model);

            return View("Checkout", viewModel);
        }

        var orderId = await this.orderService.PlaceOrderAsync(userId, model);

        //Optional confirmation.
        //return RedirectToAction("Confirmation", new { id = orderId });
        return RedirectToAction("Index", "Home");
    }

    private void RepopulateViewModel(CheckoutViewModel viewModel, CheckoutFormModel formModel)
    {
        viewModel.SelectedShippingMethodId = formModel.SelectedShippingMethodId;
        viewModel.SelectedPaymentTypeId = formModel.PaymentTypeId;

        viewModel.SelectedAddressId = formModel.SelectedAddressId;
        viewModel.UseNewAddress = formModel.UseNewAddress;

        viewModel.StreetNumber = formModel.StreetNumber;
        viewModel.AddressLine1 = formModel.AddressLine1;
        viewModel.AddressLine2 = formModel.AddressLine2;
        viewModel.PostalCode = formModel.PostalCode;
        viewModel.CountryId = formModel.CountryId;
    }
}