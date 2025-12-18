using RandomShop.Models.Order;

namespace RandomShop.Validators;

using FluentValidation;

public class CheckoutFormModelValidator : AbstractValidator<CheckoutFormModel>
{
    public CheckoutFormModelValidator()
    {
        //1.Basic CheckoutFormModelValidator Required Fields
        RuleFor(x => x.SelectedShippingMethodId)
            .NotEmpty()
            .WithMessage("Please select a shipping method.");

        RuleFor(x => x.SelectedPaymentTypeId)
            .NotEmpty()
            .WithMessage("Please select a payment method.");

        // 2. Conditional: If NOT using a new address, SelectedAddressId is REQUIRED
        RuleFor(x => x.SelectedAddressId)
            .NotEmpty()
            .When(x => !x.UseNewAddress)
            .WithMessage("Please select an address from your address book.");

        // 3. Conditional: If Using a new address, validate the nested AddressInputModel
        RuleSet("NewAddress", () =>
        {
            RuleFor(x => x.AddressInputModel)
                .NotNull()
                .When(x => x.UseNewAddress);

            RuleFor(x => x.AddressInputModel.StreetNumber)
                .NotNull()
                .InclusiveBetween(1, 99999)
                .When(x => x.UseNewAddress);

            RuleFor(x => x.AddressInputModel.AddressLine1)
                .NotEmpty()
                .Length(5, 100);

            RuleFor(x => x.AddressInputModel.AddressLine2)
                .NotEmpty()
                .Length(5, 100);

            RuleFor(x => x.AddressInputModel.PostalCode)
                .NotEmpty()
                .Matches(@"^\d{4,8}$")
                .When(x => x.UseNewAddress);

            RuleFor(x => x.AddressInputModel.CountryId)
                .NotNull()
                .When(x => x.UseNewAddress);
        });
    }
}