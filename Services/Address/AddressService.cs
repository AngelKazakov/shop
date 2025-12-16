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
            throw new InvalidOperationException("Must select a saved address or provide new address.");
        }

        if (useNewAddress)
        {
            AddressSnapshotModel snapshot = await HandleNewAddressAsync(userId, model, saveToAddressBook);
            return snapshot;
        }
        else
        {
            AddressSnapshotModel snapshot = await HandleSavedAddressAsync(userId, selectedAddressId!.Value);
            return snapshot;
        }
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

    private async Task<AddressSnapshotModel> HandleNewAddressAsync(string userId, AddressInputModel model,
        bool saveToAddressBook)
    {
        AddressSnapshotModel snapshot = new AddressSnapshotModel()
        {
            StreetNumber = model.StreetNumber,
            AddressLine1 = model.AddressLine1,
            AddressLine2 = model.AddressLine2,
            PostalCode = model.PostalCode,
            CountryId = model.CountryId,
        };

        if (saveToAddressBook)
        {
            await PersistNewAddressAsync(userId, snapshot);
        }

        return snapshot;
    }

    private async Task<AddressSnapshotModel> HandleSavedAddressAsync(string userId, int addressId)
    {
        AddressSnapshotModel? savedSnapshot = await this.context.UserAddresses.AsNoTracking()
            .Where(ua => ua.UserId == userId && ua.AddressId == addressId)
            .Select(ua => new AddressSnapshotModel()
            {
                StreetNumber = ua.Address.StreetNumber,
                AddressLine1 = ua.Address.AddressLine1,
                AddressLine2 = ua.Address.AddressLine2,
                CountryId = ua.Address.CountryId,
                PostalCode = ua.Address.PostalCode,
            })
            .FirstOrDefaultAsync();

        if (savedSnapshot == null)
        {
            throw new ArgumentException($"Saved address {addressId} not found for this user.");
        }

        return savedSnapshot;
    }

    private async Task PersistNewAddressAsync(string userId, AddressSnapshotModel snapshot)
    {
        if (snapshot.StreetNumber == null || snapshot.CountryId == null ||
            string.IsNullOrWhiteSpace(snapshot.AddressLine1)
            || string.IsNullOrWhiteSpace(snapshot.PostalCode))
        {
            throw new InvalidOperationException("Cannot save incomplete address details.");
        }

        await using var tx = await this.context.Database.BeginTransactionAsync();

        try
        {
            Data.Models.Address newAddress = new Data.Models.Address()
            {
                StreetNumber = snapshot.StreetNumber.Value,
                AddressLine1 = snapshot.AddressLine1,
                AddressLine2 = snapshot.AddressLine2,
                CountryId = snapshot.CountryId.Value,
                PostalCode = snapshot.PostalCode,
            };

            await this.context.Addresses.AddAsync(newAddress);

            UserAddress userAddressLink = new UserAddress()
            {
                UserId = userId,
                Address = newAddress,
                IsDefault = false, //Decide later how to implement it.
            };

            await this.context.UserAddresses.AddAsync(userAddressLink);

            await this.context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (Exception)
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}