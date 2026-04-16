using Microsoft.EntityFrameworkCore;
using RandomShop.Areas.Admin.Models;
using RandomShop.Data;
using RandomShop.Data.Models;
using RandomShop.Services.Images;

namespace RandomShop.Areas.Admin.Services.Product;

public class AdminProductService : IAdminProductService
{
    private readonly ShopContext context;
    private readonly IImageService imageService;

    public AdminProductService(ShopContext context, IImageService imageService)
    {
        this.context = context;
        this.imageService = imageService;
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
            "price_low" => productQuery.OrderBy(pi => pi.DiscountedPrice > 0 ? pi.DiscountedPrice : pi.Price),
            "price_high" =>
                productQuery.OrderByDescending(pi => pi.DiscountedPrice > 0 ? pi.DiscountedPrice : pi.Price),
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

    public async Task<AdminProductDetailsViewModel?> GetDetailsAsync(int? id)
    {
        if (!id.HasValue || id.Value <= 0)
        {
            return null;
        }

        int productItemId = id.Value;

        AdminProductDetailsViewModel? modelDetails = await this.context.ProductItems
            .AsNoTracking()
            .Where(pi => pi.Id == productItemId)
            .Select(pi => new AdminProductDetailsViewModel
            {
                ProductItemId = pi.Id,
                ProductId = pi.ProductId,
                Name = pi.Product.Name,
                Description = pi.Product.Description,
                SKU = pi.SKU,
                Price = pi.Price,
                DiscountedPrice = pi.DiscountedPrice,
                QuantityInStock = pi.QuantityInStock,
                CategoryName = pi.Product.ProductCategories
                    .OrderBy(pc => pc.Category.Name)
                    .Select(pc => pc.Category.Name)
                    .FirstOrDefault() ?? string.Empty,
                PromotionName = pi.Product.ProductPromotions
                    .OrderBy(pp => pp.Promotion.Name)
                    .Select(pp => pp.Promotion.Name)
                    .FirstOrDefault(),
                CreatedOnDate = pi.CreatedOnDate
            })
            .FirstOrDefaultAsync();

        if (modelDetails == null)
        {
            return null;
        }

        modelDetails.Images = await this.imageService.CreateProductImageViewModelAsync(modelDetails.ProductId);

        return modelDetails;
    }
}