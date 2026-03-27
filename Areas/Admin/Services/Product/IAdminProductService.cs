using RandomShop.Areas.Admin.Models;

namespace RandomShop.Areas.Admin.Services.Product;

public interface IAdminProductService
{
    public Task<IEnumerable<AdminProductListItemViewModel>> GetAllProductsAsync();
}