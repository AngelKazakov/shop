using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Models.Address;

namespace RandomShop.Services.Address;

public class AddressService : IAddressService
{
    private readonly ShopContext context;

    public AddressService(ShopContext context)
    {
        this.context = context;
    }

    public async Task<int> HandleOrderAddressAsync(string userId, AddressInputModel model, bool saveToAddressBook,
        int selectedAddressId, bool useNewAddress)
    {
        int addressId;

        if (saveToAddressBook || useNewAddress)
        {
            Data.Models.Address newAddress = new Data.Models.Address()
            {
                StreetNumber = model.StreetNumber.Value,
                AddressLine1 = model.AddressLine1,
                AddressLine2 = model.AddressLine2,
                CountryId = model.CountryId.Value,
                PostalCode = model.PostalCode,
            };

            await this.context.Addresses.AddAsync(newAddress);
            await this.context.SaveChangesAsync();

            UserAddress userAddressLink = new UserAddress()
            {
                AddressId = newAddress.Id,
                UserId = userId,
            };

            await this.context.UserAddresses.AddAsync(userAddressLink);
            await this.context.SaveChangesAsync();

            addressId = newAddress.Id;
        }
        else
        {
            if (selectedAddressId == null)
            {
                throw new InvalidOperationException("SelectedAddressId is missing for saved address choice.");
            }

            return selectedAddressId;
        }

        return addressId;
    }

    public Dictionary<string, string> ValidateAddressSelection(AddressInputModel? model,bool useNewAddress, int? selectedAddressId)
    {
        var errors = new Dictionary<string, string>();
        const string modelPrefix = nameof(AddressInputModel);

        if (useNewAddress)
        {
            if (model == null)
            {
                errors.Add(modelPrefix, "New address details are required.");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(model.PostalCode))
                errors.Add($"{modelPrefix}.{nameof(model.PostalCode)}", "Postal code is required.");

            if (!model.StreetNumber.HasValue)
                errors.Add($"{modelPrefix}.{nameof(model.StreetNumber)}", "Street number is required.");

        }
        else 
        {
            if (!selectedAddressId.HasValue || selectedAddressId.Value <= 0)
            {
                errors.Add(nameof(selectedAddressId), "Please choose a saved address.");
            }
        }

        return errors;
    }
}