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
        int selectedAddressId)
    {
        int addressId;

        if (saveToAddressBook)
        {
            Data.Models.Address newAddress = new Data.Models.Address()
            {
                StreetNumber = model.StreetNumber.Value,
                AddressLine1 = model.AddressLine1,
                AddressLine2 = model.AddressLine2,
                CountryId = model.CountryId.Value,
                PostalCode = model.PostalCode.Value,
            };

            await this.context.Addresses.AddAsync(newAddress);
            await this.context.SaveChangesAsync();

            var userAddressLink = new UserAddress()
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
}