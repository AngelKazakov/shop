using Microsoft.EntityFrameworkCore;
using RandomShop.Areas.Admin.Models;
using RandomShop.Data;
using RandomShop.Data.Models;

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
                CategoryName = pi.Product.ProductCategories
                    .Select(pc => pc.Category.Name)
                    .FirstOrDefault() ?? string.Empty,
                Price = pi.Price,
                DiscountedPrice = pi.DiscountedPrice,
                Quantity = pi.QuantityInStock,
                HasPromotion = pi.Product.ProductPromotions.Any(),
                CreatedOn = pi.CreatedOnDate,
            })
            .ToListAsync();

        return products;
    }

    public async Task<AdminProductListQueryModel> GetPagedAsync(AdminProductListQueryModel query)
    {
        query.Page = Math.Max(1, query.Page);
        query.PageSize = query.PageSize <= 0
            ? AdminProductListQueryModel.DefaultPageSize
            : Math.Min(query.PageSize, AdminProductListQueryModel.MaxPageSize);
        query.SortBy = string.IsNullOrWhiteSpace(query.SortBy) ? "Newest" : query.SortBy;
        query.StockFilter = string.IsNullOrWhiteSpace(query.StockFilter) ? "all" : query.StockFilter;
        query.PromotionFilter = string.IsNullOrWhiteSpace(query.PromotionFilter) ? "all" : query.PromotionFilter;

        IQueryable<ProductItem> productQuery = this.context.ProductItems
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            string searchTerm = query.SearchTerm.Trim();

            productQuery = productQuery.Where(pi =>
                pi.Product.Name.Contains(searchTerm) ||
                pi.SKU.Contains(searchTerm) ||
                pi.Product.ProductCategories.Any(pc => pc.Category.Name.Contains(searchTerm)));
        }

        if (query.CategoryId.HasValue)
        {
            int categoryId = query.CategoryId.Value;
            productQuery = productQuery.Where(pi =>
                pi.Product.ProductCategories.Any(pc => pc.CategoryId == categoryId));
        }

        if (query.PromotionId.HasValue)
        {
            int promotionId = query.PromotionId.Value;
            productQuery = productQuery.Where(pi =>
                pi.Product.ProductPromotions.Any(pp => pp.PromotionId == promotionId));
        }

        if (query.InStockOnly == true)
        {
            productQuery = productQuery.Where(pi => pi.QuantityInStock > 0);
        }

        productQuery = query.StockFilter.ToLowerInvariant() switch
        {
            "low" => productQuery.Where(pi => pi.QuantityInStock >= 0 && pi.QuantityInStock <= 5),
            "available" => productQuery.Where(pi => pi.QuantityInStock >= 6),
            "empty" => productQuery.Where(pi => pi.QuantityInStock == 0),
            _ => productQuery
        };

        productQuery = query.PromotionFilter.ToLowerInvariant() switch
        {
            "promo" => productQuery.Where(pi => pi.Product.ProductPromotions.Any()),
            "none" => productQuery.Where(pi => !pi.Product.ProductPromotions.Any()),
            _ => productQuery
        };

        productQuery = query.SortBy.ToLowerInvariant() switch
        {
            "oldest" => productQuery.OrderBy(pi => pi.CreatedOnDate),
            "name" => productQuery.OrderBy(pi => pi.Product.Name),
            "name_desc" => productQuery.OrderByDescending(pi => pi.Product.Name),
            "price_low" => productQuery.OrderBy(pi => pi.DiscountedPrice),
            "price_high" => productQuery.OrderByDescending(pi => pi.DiscountedPrice),
            "stock_low" => productQuery.OrderBy(pi => pi.QuantityInStock),
            "stock_high" => productQuery.OrderByDescending(pi => pi.QuantityInStock),
            _ => productQuery.OrderByDescending(pi => pi.CreatedOnDate)
        };

        query.TotalItems = await productQuery.CountAsync();

        if (query.TotalItems > 0 && query.Page > query.TotalPages)
        {
            query.Page = query.TotalPages;
        }

        query.Items = await productQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(pi => new AdminProductListItemViewModel
            {
                ProductItemId = pi.Id,
                ProductId = pi.ProductId,
                Name = pi.Product.Name,
                SKU = pi.SKU,
                CategoryName = pi.Product.ProductCategories
                    .Select(pc => pc.Category.Name)
                    .FirstOrDefault() ?? string.Empty,
                Price = pi.Price,
                DiscountedPrice = pi.DiscountedPrice,
                Quantity = pi.QuantityInStock,
                HasPromotion = pi.Product.ProductPromotions.Any(),
                CreatedOn = pi.CreatedOnDate,
            })
            .ToListAsync();

        return query;
    }
}