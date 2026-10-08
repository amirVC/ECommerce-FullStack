using ECommerceAPI.Common;
using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Extensions;
using ECommerceAPI.Models;
using ECommerceAPI.Models.QueryParameters;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly IReviewService _reviewService;
    private readonly ICampaignService _campaignService;

    public ProductService(
        AppDbContext context,
        IReviewService reviewService,
        ICampaignService campaignService)
    {
        _context = context;
        _reviewService = reviewService;
        _campaignService = campaignService;
    }


    // GET ALL PRODUCTS


    public async Task<PagedResult<ProductResponseDto>>
        GetAllAsync(ProductQueryParameters query)
    {
        var productsQuery =
            _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable();


        // SEARCH


        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            productsQuery =
                productsQuery.Where(p =>
                    EF.Functions.ILike(
                        p.Name,
                        $"%{query.Search}%") ||
                    EF.Functions.ILike(
                        p.SKU,
                        $"%{query.Search}%"));
        }


        // CATEGORY FILTER


        if (query.CategoryId.HasValue)
        {
            productsQuery =
                productsQuery.Where(p =>
                    p.CategoryId ==
                    query.CategoryId.Value);
        }


        // SORT

        productsQuery =
            query.SortBy?.ToLower() switch
            {
                "priceasc" =>
                    productsQuery
                        .OrderBy(p => p.Price)
                        .ThenByDescending(p => p.Id),

                "pricedesc" =>
                    productsQuery
                        .OrderByDescending(p => p.Price)
                        .ThenByDescending(p => p.Id),

                "name" =>
                    productsQuery
                        .OrderBy(p => p.Name)
                        .ThenByDescending(p => p.Id),

                "newest" =>
                    productsQuery
                        .OrderByDescending(p => p.CreatedAt)
                        .ThenByDescending(p => p.Id),

                _ =>
                    productsQuery
                        .OrderByDescending(p => p.CreatedAt)
                        .ThenByDescending(p => p.Id)
            };

        // TOTAL COUNT


        var totalItems =
            await productsQuery.CountAsync();


        // PAGINATION


        productsQuery =
            productsQuery
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize);

        var products =
            await productsQuery.ToListAsync();


        // EMPTY RESULT


        if (products.Count == 0)
        {
            return new PagedResult<ProductResponseDto>
            {
                Items =
                    new List<ProductResponseDto>(),

                Page =
                    query.Page,

                PageSize =
                    query.PageSize,

                TotalItems =
                    totalItems,

                TotalPages =
                    (int)Math.Ceiling(
                        totalItems /
                        (double)query.PageSize)
            };
        }


        // PRODUCT IDS


        var productIds =
            products
                .Select(p => p.Id)
                .ToList();


        // REVIEW STATISTICS

        var reviewStats =
            await _context.Reviews
                .AsNoTracking()
                .Where(r =>
                    productIds.Contains(r.ProductId) &&
                    r.Status == ReviewStatus.Approved)
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId =
                        g.Key,

                    Average =
                        g.Average(r => r.Rating),

                    Count =
                        g.Count()
                })
                .ToDictionaryAsync(
                    x => x.ProductId,
                    x => x);


        // BATCH CAMPAIGNS

        var campaignKeys =
            products
                .Select(p => (
                    ProductId: p.Id,
                    CategoryId: p.CategoryId
                ))
                .ToList();

        var activeCampaigns =
            await _campaignService
                .GetActiveCampaignsForProductsAsync(
                    campaignKeys);


        // BUILD DTOs


        var items =
            new List<ProductResponseDto>(
                products.Count);

        foreach (var product in products)
        {
            var dto =
                product.ToResponseDto();


            // ORIGINAL PRICE


            var originalPrice =
                product.Price;


            // SALE PRICE


            var salePrice =
                product.IsOnSale()
                    ? product.SalePrice!.Value
                    : originalPrice;

            dto.Price =
                originalPrice;

            dto.IsOnSale =
                product.IsOnSale();

            dto.SalePrice =
                product.SalePrice;

            dto.SaleStartDate =
                product.SaleStartDate;

            dto.SaleEndDate =
                product.SaleEndDate;

            dto.EffectivePrice =
                salePrice;


            // CAMPAIGN

            activeCampaigns.TryGetValue(
                product.Id,
                out var campaign);

            var campaignDiscount =
                CampaignService.ComputeDiscount(campaign, salePrice);

            dto.CampaignDiscount =
                campaignDiscount;

            dto.HasCampaign =
                campaignDiscount > 0;

            dto.CampaignName =
                campaign?.Name;


            // FINAL PRICE


            dto.FinalPrice =
                Math.Max(
                    0m,
                    salePrice -
                    campaignDiscount);

            dto.TotalDiscountAmount =
                Math.Max(
                    0m,
                    originalPrice -
                    dto.FinalPrice);


            // REVIEWS


            if (reviewStats.TryGetValue(
                    product.Id,
                    out var stats))
            {
                dto.AverageRating =
                    Math.Round(
                        stats.Average,
                        1);

                dto.ReviewCount =
                    stats.Count;
            }

            items.Add(dto);
        }


        // RESULT


        return new PagedResult<ProductResponseDto>
        {
            Items =
                items,

            Page =
                query.Page,

            PageSize =
                query.PageSize,

            TotalItems =
                totalItems,

            TotalPages =
                (int)Math.Ceiling(
                    totalItems /
                    (double)query.PageSize)
        };
    }


    // GET PRODUCT BY ID


    public async Task<ProductResponseDto?>
        GetByIdAsync(int id)
    {
        var product =
            await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p =>
                    p.Images
                        .OrderBy(i =>
                            i.DisplayOrder))
                .FirstOrDefaultAsync(
                    p => p.Id == id);

        if (product == null)
            return null;

        var dto =
            product.ToResponseDto();

        var salePrice =
            product.IsOnSale()
                ? product.SalePrice!.Value
                : product.Price;

        dto.Price =
            product.Price;

        dto.IsOnSale =
            product.IsOnSale();

        dto.SalePrice =
            product.SalePrice;

        dto.SaleStartDate =
            product.SaleStartDate;

        dto.SaleEndDate =
            product.SaleEndDate;

        dto.EffectivePrice =
            salePrice;


        // SINGLE PRODUCT CAMPAIGN


        var campaign =
            await _campaignService
                .GetActiveCampaignForProductAsync(
                    id);

        var campaignDiscount =
            CampaignService.ComputeDiscount(campaign, salePrice);

        dto.CampaignDiscount =
            campaignDiscount;

        dto.HasCampaign =
            campaignDiscount > 0;

        dto.CampaignName =
            campaign?.Name;

        dto.FinalPrice =
            Math.Max(
                0m,
                salePrice -
                campaignDiscount);

        dto.TotalDiscountAmount =
            Math.Max(
                0m,
                product.Price -
                dto.FinalPrice);


        // REVIEWS


        var summary =
            await _reviewService
                .GetProductReviewSummaryAsync(
                    id);

        dto.AverageRating =
            summary.AverageRating;

        dto.ReviewCount =
            summary.TotalReviews;

        return dto;
    }


    // GET PRODUCT BY SKU


    public async Task<ProductResponseDto?>
        GetBySkuAsync(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return null;

        var normalizedSku = sku.Trim().ToUpperInvariant();

        var product =
            await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p =>
                    p.Images
                        .OrderBy(i =>
                            i.DisplayOrder))
                .FirstOrDefaultAsync(
                    p => p.SKU == normalizedSku);

        if (product == null)
            return null;

        return await GetByIdAsync(product.Id);
    }


    // EXISTING INTERFACE PLACEHOLDERS


    public Task<IEnumerable<ProductResponseDto>>
        GetByCategoryAsync(int categoryId)
    {
        throw new NotImplementedException();
    }

    public Task<ProductResponseDto>
        CreateAsync(ProductDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ProductResponseDto?>
        UpdateAsync(
            int id,
            ProductDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<bool>
        DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }


    // ADMIN GET BY ID


    public async Task<AdminUpdateProductDto?>
        GetAdminByIdAsync(int id)
    {
        var product =
            await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.Id == id);

        if (product == null)
            return null;

        return new AdminUpdateProductDto
        {
            SKU =
                product.SKU,

            Name =
                product.Name,

            Description =
                product.Description,

            Price =
                product.Price,

            Stock =
                product.Stock,

            ImageUrl =
                product.ImageUrl,

            CategoryId =
                product.CategoryId,

            SalePrice =
                product.SalePrice,

            SaleStartDate =
                product.SaleStartDate,

            SaleEndDate =
                product.SaleEndDate
        };
    }


    // RELATED PRODUCTS


    public async Task<List<ProductResponseDto>>
        GetRelatedAsync(
            int id,
            int take = 4)
    {
        var currentProduct =
            await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.Id == id);

        if (currentProduct == null)
        {
            return new List<ProductResponseDto>();
        }

        var relatedProducts =
            await _context.Products
                .AsNoTracking()
                .Where(p =>
                    p.CategoryId ==
                    currentProduct.CategoryId &&
                    p.Id != id)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .OrderByDescending(p => p.Id)
                .Take(take)
                .ToListAsync();

        if (relatedProducts.Count == 0)
        {
            return new List<ProductResponseDto>();
        }


        // BATCH CAMPAIGNS


        var campaignKeys =
            relatedProducts
                .Select(p => (
                    ProductId: p.Id,
                    CategoryId: p.CategoryId
                ))
                .ToList();

        var activeCampaigns =
            await _campaignService
                .GetActiveCampaignsForProductsAsync(
                    campaignKeys);

        var items =
            new List<ProductResponseDto>(
                relatedProducts.Count);

        foreach (var product in relatedProducts)
        {
            var dto =
                product.ToResponseDto();

            var originalPrice =
                product.Price;

            var salePrice =
                product.IsOnSale()
                    ? product.SalePrice!.Value
                    : product.Price;

            dto.Price =
                originalPrice;

            dto.IsOnSale =
                product.IsOnSale();

            dto.SalePrice =
                product.SalePrice;

            dto.SaleStartDate =
                product.SaleStartDate;

            dto.SaleEndDate =
                product.SaleEndDate;

            dto.EffectivePrice =
                salePrice;


            // CAMPAIGN


            activeCampaigns.TryGetValue(
                product.Id,
                out var campaign);

            var campaignDiscount =
                CampaignService.ComputeDiscount(campaign, salePrice);

            dto.CampaignDiscount =
                campaignDiscount;

            dto.HasCampaign =
                campaignDiscount > 0;

            dto.CampaignName =
                campaign?.Name;


            // FINAL PRICE


            dto.FinalPrice =
                Math.Max(
                    0m,
                    salePrice -
                    campaignDiscount);

            dto.TotalDiscountAmount =
                Math.Max(
                    0m,
                    originalPrice -
                    dto.FinalPrice);

            items.Add(dto);
        }

        return items;
    }
}