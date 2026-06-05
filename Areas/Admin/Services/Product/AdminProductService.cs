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

    public async Task<AdminEditProductFormModel?> GetEditFormAsync(int? productItemId)
    {
        if (!productItemId.HasValue || productItemId.Value <= 0)
        {
            return null;
        }

        int id = productItemId.Value;

        AdminEditProductFormModel? model = await this.GetBaseEditFormAsync(id);

        if (model == null)
        {
            return null;
        }

        await this.PopulateSelectedVariationDataAsync(model);
        await this.PopulateEditLookupsAsync(model);
        await this.PopulateExistingImagesAsync(model);

        return model;
    }

    public async Task<bool> UpdateAsync(AdminEditProductFormModel model)
    {
        ProductItem? productToUpdate = await this.context.ProductItems
            .Include(pi => pi.Product)
            .ThenInclude(p => p.ProductCategories)
            .Include(pi => pi.Product)
            .ThenInclude(p => p.ProductPromotions)
            .Include(pi => pi.ProductConfigurations)
            .FirstOrDefaultAsync(pi => pi.Id == model.ProductItemId);

        if (productToUpdate == null)
        {
            return false;
        }

        await this.UpdateBasicProductData(productToUpdate, model);
        this.UpdateCategory(productToUpdate.Product, model.CategoryId);
        this.UpdatePromotion(productToUpdate.Product, model.PromotionId);
        this.UpdateProductVariationOptions(productToUpdate, model.SelectedVariationOptionIds);

        await this.context.SaveChangesAsync();

        return true;
    }

    public async Task RebuildEditFormAsync(AdminEditProductFormModel model)
    {
        await this.PopulateEditLookupsAsync(model);
        await this.PopulateExistingImagesAsync(model);
    }

    private async Task UpdateBasicProductData(ProductItem productItem, AdminEditProductFormModel model)
    {
        productItem.Product.Name = model.Name;
        productItem.Product.Description = model.Description;

        productItem.SKU = model.SKU;
        productItem.QuantityInStock = model.QuantityInStock;
        productItem.Price = model.Price;
        productItem.DiscountedPrice = model.PromotionId.HasValue && model.PromotionId.Value > 0
            ? await this.ApplyPromotionAsync(model.PromotionId.Value, model.Price)
            : 0;
    }

    private void UpdateCategory(Data.Models.Product product, int categoryId)
    {
        product.ProductCategories.Clear();

        product.ProductCategories.Add(new ProductCategory()
        {
            CategoryId = categoryId,
            ProductId = product.Id
        });
    }

    private async Task<decimal> ApplyPromotionAsync(int promotionId, decimal productPrice)
    {
        int? promotionDiscountRate = await this.context.Promotions
            .AsNoTracking()
            .Where(p => p.Id == promotionId)
            .Select(p => (int?)p.DiscountRate)
            .FirstOrDefaultAsync();

        if (!promotionDiscountRate.HasValue || promotionDiscountRate.Value <= 0)
        {
            return 0;
        }

        decimal discountRate = promotionDiscountRate.Value / 100m;
        decimal discountedPrice = productPrice * (1 - discountRate);

        return Math.Round(discountedPrice, 2);
    }

    private void UpdatePromotion(Data.Models.Product product, int? promotionId)
    {
        product.ProductPromotions.Clear();

        if (promotionId.HasValue && promotionId.Value > 0)
        {
            product.ProductPromotions.Add(new ProductPromotion()
            {
                PromotionId = promotionId.Value,
                ProductId = product.Id
            });
        }
    }

    private void UpdateProductVariationOptions(ProductItem productItem, List<int> selectedVariationOptionIds)
    {
        productItem.ProductConfigurations.Clear();

        if (selectedVariationOptionIds.Count == 0)
        {
            return;
        }

        foreach (var variationOptionId in selectedVariationOptionIds.Distinct())
        {
            productItem.ProductConfigurations.Add(new ProductConfiguration
            {
                ProductItemId = productItem.Id,
                VariationOptionId = variationOptionId
            });
        }
    }

    private async Task<AdminEditProductFormModel?> GetBaseEditFormAsync(int productItemId)
    {
        return await this.context.ProductItems
            .AsNoTracking()
            .Where(pi => pi.Id == productItemId)
            .Select(pi => new AdminEditProductFormModel
            {
                ProductItemId = pi.Id,
                ProductId = pi.ProductId,
                Name = pi.Product.Name,
                Description = pi.Product.Description,
                SKU = pi.SKU,
                QuantityInStock = pi.QuantityInStock,
                Price = pi.Price,
                DiscountedPrice = pi.DiscountedPrice,
                CategoryId = pi.Product.ProductCategories
                    .Select(pc => pc.CategoryId)
                    .FirstOrDefault(),
                PromotionId = pi.Product.ProductPromotions
                    .Select(pp => (int?)pp.PromotionId)
                    .FirstOrDefault(),
                SelectedVariationOptionIds = pi.ProductConfigurations
                    .Select(pc => pc.VariationOptionId)
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    private async Task PopulateSelectedVariationDataAsync(AdminEditProductFormModel model)
    {
        var selectedVariationOptions = await this.context.ProductConfigurations
            .AsNoTracking()
            .Where(pc => pc.ProductItemId == model.ProductItemId)
            .Select(pc => new
            {
                pc.VariationOption.VariationId,
                VariationName = pc.VariationOption.Variation.Name,
                pc.VariationOptionId,
                OptionValue = pc.VariationOption.Value
            })
            .ToListAsync();

        model.SelectedVariationOptions = selectedVariationOptions
            .GroupBy(vo => vo.VariationId)
            .ToDictionary(group => group.Key, group => (int?)group.First().VariationOptionId);

        model.ExistingVariationOptions = selectedVariationOptions
            .GroupBy(vo => vo.VariationName)
            .ToDictionary(group => group.Key, group => group.Select(vo => vo.OptionValue).ToList());
    }

    private async Task PopulateEditLookupsAsync(AdminEditProductFormModel model)
    {
        model.Categories = await this.context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new RandomShop.Models.Category.CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync();

        model.Promotions = await this.context.Promotions
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new RandomShop.Models.Promotion.PromotionViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                DiscountRate = p.DiscountRate,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            })
            .ToListAsync();

        List<int> selectedVariationIds = model.SelectedVariationOptions.Keys.ToList();

        model.AllVariationOptions = await this.context.Variations
            .AsNoTracking()
            .Where(v => v.CategoryId == model.CategoryId || selectedVariationIds.Contains(v.Id))
            .OrderBy(v => v.Name)
            .Select(v => new RandomShop.Models.Variation.VariationOptionViewModel
            {
                VariationId = v.Id,
                VariationName = v.Name,
                VariationOptions = v.VariationOptions
                    .OrderBy(vo => vo.Value)
                    .Select(vo => new RandomShop.Models.Variation.VariationOptionFormViewModel
                    {
                        VariationOptionId = vo.Id,
                        Value = vo.Value
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    private async Task PopulateExistingImagesAsync(AdminEditProductFormModel model)
    {
        model.ExistingImages =
            await this.imageService.CreateProductImageViewModelAsync(model.ProductId);
    }
}