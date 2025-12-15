using Microsoft.EntityFrameworkCore;
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

    public async Task<AddressSnapshotModel> HandleOrderAddressAsync(string userId, AddressInputModel model,
        bool saveToAddressBook, int? selectedAddressId, bool useNewAddress)
    {
        if (!useNewAddress && selectedAddressId == null)
        {
            throw new InvalidOperationException(
                "Must select a saved address or provide a new address.");
        }

        if (useNewAddress)
        {
            AddressSnapshotModel snapshot = new AddressSnapshotModel
            {
                StreetNumber = model.StreetNumber,
                AddressLine1 = model.AddressLine1,
                AddressLine2 = model.AddressLine2,
                PostalCode = model.PostalCode,
                CountryId = model.CountryId
            };

            if (saveToAddressBook)
            {
                if (snapshot.StreetNumber == null ||
                    snapshot.CountryId == null ||
                    string.IsNullOrWhiteSpace(snapshot.AddressLine1) ||
                    string.IsNullOrWhiteSpace(snapshot.PostalCode))
                {
                    throw new InvalidOperationException(
                        "New address details are incomplete.");
                }

                await using var tx = await context.Database.BeginTransactionAsync();

                try
                {
                    var newAddress = new RandomShop.Data.Models.Address
                    {
                        StreetNumber = snapshot.StreetNumber.Value,
                        AddressLine1 = snapshot.AddressLine1,
                        AddressLine2 = snapshot.AddressLine2,
                        PostalCode = snapshot.PostalCode,
                        CountryId = snapshot.CountryId.Value
                    };

                    context.Addresses.Add(newAddress);

                    UserAddress userAddressLink = new UserAddress
                    {
                        UserId = userId,
                        Address = newAddress,
                        IsDefault = false // decide later if I want to set default
                    };

                    context.UserAddresses.Add(userAddressLink);

                    await context.SaveChangesAsync();
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }

            return snapshot;
        }

        AddressSnapshotModel? savedSnapshot = await context.UserAddresses
            .AsNoTracking()
            .Where(ua => ua.UserId == userId && ua.AddressId == selectedAddressId!.Value)
            .Select(ua => new AddressSnapshotModel
            {
                StreetNumber = ua.Address.StreetNumber,
                AddressLine1 = ua.Address.AddressLine1,
                AddressLine2 = ua.Address.AddressLine2,
                PostalCode = ua.Address.PostalCode,
                CountryId = ua.Address.CountryId
            })
            .FirstOrDefaultAsync();

        if (savedSnapshot == null)
        {
            throw new ArgumentException(
                $"Saved address {selectedAddressId} not found for this user.");
        }

        return savedSnapshot;
    }


    public Dictionary<string, string> ValidateAddressSelection(AddressInputModel? model, bool useNewAddress,
        int? selectedAddressId)
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