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
        RuleFor(x => x.Address)
            .NotNull()
            .When(x => x.UseNewAddress);

        RuleFor(x => x.Address.StreetNumber)
            .NotNull()
            .InclusiveBetween(1, 99999)
            .When(x => x.UseNewAddress);

        RuleFor(x => x.Address.AddressLine1)
            .NotEmpty()
            .When(x => x.UseNewAddress)
            .Length(5, 100);

        RuleFor(x => x.Address.AddressLine2)
            .MaximumLength(100);

        RuleFor(x => x.Address.PostalCode)
            .NotEmpty()
            .When(x => x.UseNewAddress)
            .Matches(@"^\d{4,8}$")
            .WithMessage("Postal code must be 4 to 8 digits.");

        RuleFor(x => x.Address.CountryId)
            .NotNull()
            .When(x => x.UseNewAddress);
    }
}