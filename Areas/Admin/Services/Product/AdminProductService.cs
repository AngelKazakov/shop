using Microsoft.EntityFrameworkCore;
using RandomShop.Areas.Admin.Models;
using RandomShop.Data;

namespace RandomShop.Areas.Admin.Services.Product;

public class AdminProductService : IAdminProductService
{
    private readonly ShopContext context;

    public AdminProductService(ShopContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<AdminProductListItemViewModel>> GetAllProductsAsync()
    {
        List<AdminProductListItemViewModel> products = await this.context.ProductItems
            .AsNoTracking()
            .Select(pi => new AdminProductListItemViewModel
            {
                ProductItemId = pi.Id,
                ProductId = pi.ProductId,
                Name = pi.Product.Name,
                SKU = pi.SKU,
                CategoryName = pi.Product.ProductCategories.Select(pc => pc.Category.Name).FirstOrDefault() ??
                               string.Empty,
                Price = pi.Price,
                DiscountedPrice = pi.DiscountedPrice,
                Quantity = pi.QuantityInStock,
                HasPromotion = pi.Product.ProductPromotions.Any(),
                CreatedOn = pi.CreatedOnDate,
            }).ToListAsync();

        return products;
    }
}