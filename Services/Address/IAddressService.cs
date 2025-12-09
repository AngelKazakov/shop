using RandomShop.Models.Address;

namespace RandomShop.Services.Address;

public interface IAddressService
{
    public Task<int> HandleOrderAddressAsync(string userId, AddressInputModel model, bool saveToAddressBook,
        int selectedAddressId);
}