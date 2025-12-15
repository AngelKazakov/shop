using RandomShop.Models.Address;
using RandomShop.Models.Order;

namespace RandomShop.Services.Address;

public interface IAddressService
{
    public Task<AddressSnapshotModel> HandleOrderAddressAsync(string userId, AddressInputModel model, bool saveToAddressBook,
        int? selectedAddressId, bool useNewAddress
    );

    public Dictionary<string, string> ValidateAddressSelection(AddressInputModel? model, bool useNewAddress,
        int? selectedAddressId);
}