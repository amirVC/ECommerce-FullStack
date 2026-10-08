using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services;

public class CampaignService : ICampaignService
{
    private readonly AppDbContext _context;

    public CampaignService(AppDbContext context)
    {
        _context = context;
    }


    // GET ALL CAMPAIGNS


    public async Task<List<CampaignDto>> GetAllAsync()
    {
        return await _context.Campaigns
            .AsNoTracking()
            .OrderByDescending(c => c.Id)
            .Select(c => new CampaignDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                DiscountType = (int)c.DiscountType,
                DiscountValue = c.DiscountValue,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsActive = c.IsActive,

                ProductIds = c.CampaignProducts
                    .Select(cp => cp.ProductId)
                    .ToList(),

                CategoryIds = c.CampaignCategories
                    .Select(cc => cc.CategoryId)
                    .ToList()
            })
            .ToListAsync();
    }


    // GET CAMPAIGN BY ID


    public async Task<CampaignDto?> GetByIdAsync(int id)
    {
        return await _context.Campaigns
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CampaignDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                DiscountType = (int)c.DiscountType,
                DiscountValue = c.DiscountValue,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsActive = c.IsActive,

                ProductIds = c.CampaignProducts
                    .Select(cp => cp.ProductId)
                    .ToList(),

                CategoryIds = c.CampaignCategories
                    .Select(cc => cc.CategoryId)
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }


    // CREATE CAMPAIGN


    public async Task<CampaignDto> CreateAsync(
        AdminCreateCampaignDto dto)
    {
        var startDate =
            NormalizeToUtc(dto.StartDate);

        var endDate =
            NormalizeToUtc(dto.EndDate);

        ValidateCampaign(
            dto.Name,
            dto.DiscountType,
            dto.DiscountValue,
            startDate,
            endDate);

        var campaign = new Campaign
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),

            DiscountType =
                (CampaignDiscountType)dto.DiscountType,

            DiscountValue =
                dto.DiscountValue,

            StartDate = startDate,
            EndDate = endDate,
            IsActive = dto.IsActive
        };

        foreach (var productId in
                 dto.ProductIds.Distinct())
        {
            campaign.CampaignProducts.Add(
                new CampaignProduct
                {
                    ProductId = productId
                });
        }

        foreach (var categoryId in
                 dto.CategoryIds.Distinct())
        {
            campaign.CampaignCategories.Add(
                new CampaignCategory
                {
                    CategoryId = categoryId
                });
        }

        _context.Campaigns.Add(campaign);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(campaign.Id)
               ?? throw new InvalidOperationException(
                   "Campaign could not be loaded after creation.");
    }


    // UPDATE CAMPAIGN


    public async Task<CampaignDto?> UpdateAsync(
        int id,
        AdminUpdateCampaignDto dto)
    {
        var startDate =
            NormalizeToUtc(dto.StartDate);

        var endDate =
            NormalizeToUtc(dto.EndDate);

        ValidateCampaign(
            dto.Name,
            dto.DiscountType,
            dto.DiscountValue,
            startDate,
            endDate);

        var campaign =
            await _context.Campaigns
                .Include(c => c.CampaignProducts)
                .Include(c => c.CampaignCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null)
            return null;

        campaign.Name =
            dto.Name.Trim();

        campaign.Description =
            dto.Description?.Trim();

        campaign.DiscountType =
            (CampaignDiscountType)dto.DiscountType;

        campaign.DiscountValue =
            dto.DiscountValue;

        campaign.StartDate =
            startDate;

        campaign.EndDate =
            endDate;

        campaign.IsActive =
            dto.IsActive;

        campaign.CampaignProducts.Clear();
        campaign.CampaignCategories.Clear();

        foreach (var productId in
                 dto.ProductIds.Distinct())
        {
            campaign.CampaignProducts.Add(
                new CampaignProduct
                {
                    CampaignId = campaign.Id,
                    ProductId = productId
                });
        }

        foreach (var categoryId in
                 dto.CategoryIds.Distinct())
        {
            campaign.CampaignCategories.Add(
                new CampaignCategory
                {
                    CampaignId = campaign.Id,
                    CategoryId = categoryId
                });
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(campaign.Id);
    }


    // DELETE CAMPAIGN


    public async Task<bool> DeleteAsync(int id)
    {
        var campaign =
            await _context.Campaigns
                .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null)
            return false;

        _context.Campaigns.Remove(campaign);

        await _context.SaveChangesAsync();

        return true;
    }


    // GET ACTIVE CAMPAIGN FOR ONE PRODUCT


    public async Task<CampaignDto?>
        GetActiveCampaignForProductAsync(int productId)
    {
        var now =
            DateTime.UtcNow;

        return await _context.Campaigns
            .AsNoTracking()
            .Where(c =>
                c.IsActive &&
                c.StartDate <= now &&
                c.EndDate >= now &&
                (
                    c.CampaignProducts.Any(cp =>
                        cp.ProductId == productId)

                    ||

                    c.CampaignCategories.Any(cc =>
                        cc.Category.Products.Any(p =>
                            p.Id == productId))
                ))
            .OrderByDescending(c => c.Id)
            .Select(c => new CampaignDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                DiscountType = (int)c.DiscountType,
                DiscountValue = c.DiscountValue,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsActive = c.IsActive,

                ProductIds = c.CampaignProducts
                    .Select(cp => cp.ProductId)
                    .ToList(),

                CategoryIds = c.CampaignCategories
                    .Select(cc => cc.CategoryId)
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }




    public async Task<Dictionary<int, CampaignDto>>
        GetActiveCampaignsForProductsAsync(
            IEnumerable<(int ProductId, int CategoryId)> products)
    {
        var productList =
            products.ToList();

        if (productList.Count == 0)
        {
            return new Dictionary<int, CampaignDto>();
        }

        var productIds =
            productList
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

        var categoryIds =
            productList
                .Select(x => x.CategoryId)
                .Distinct()
                .ToList();

        var now =
            DateTime.UtcNow;




        var campaigns =
            await _context.Campaigns
                .AsNoTracking()
                .Where(c =>
                    c.IsActive &&
                    c.StartDate <= now &&
                    c.EndDate >= now &&
                    (
                        c.CampaignProducts.Any(cp =>
                            productIds.Contains(cp.ProductId))

                        ||

                        c.CampaignCategories.Any(cc =>
                            categoryIds.Contains(cc.CategoryId))
                    ))
                .OrderByDescending(c => c.Id)
                .Select(c => new CampaignDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DiscountType =
                        (int)c.DiscountType,
                    DiscountValue =
                        c.DiscountValue,
                    StartDate =
                        c.StartDate,
                    EndDate =
                        c.EndDate,
                    IsActive =
                        c.IsActive,

                    ProductIds =
                        c.CampaignProducts
                            .Select(cp => cp.ProductId)
                            .ToList(),

                    CategoryIds =
                        c.CampaignCategories
                            .Select(cc => cc.CategoryId)
                            .ToList()
                })
                .ToListAsync();


        // MATCH CAMPAIGNS IN MEMORY


        var result =
            new Dictionary<int, CampaignDto>();

        foreach (var product in productList)
        {
            var campaign =
                campaigns.FirstOrDefault(c =>
                    c.ProductIds.Contains(
                        product.ProductId)
                    ||
                    c.CategoryIds.Contains(
                        product.CategoryId));

            if (campaign != null)
            {
                result[product.ProductId] =
                    campaign;
            }
        }

        return result;
    }




    public async Task<decimal> CalculateDiscountAsync(
        Product product,
        decimal basePrice)
    {
        var campaign =
            await GetActiveCampaignForProductAsync(
                product.Id);

        if (campaign == null)
            return 0m;

        return ComputeDiscount(campaign, basePrice);
    }




    public static decimal ComputeDiscount(
        CampaignDto? campaign,
        decimal basePrice)
    {
        if (campaign == null)
            return 0m;

        decimal discount;

        if (campaign.DiscountType ==
            (int)CampaignDiscountType.Percentage)
        {
            discount =
                basePrice *
                (campaign.DiscountValue / 100m);
        }
        else
        {
            discount =
                campaign.DiscountValue;
        }

        discount =
            Math.Min(
                discount,
                basePrice);

        return Math.Round(
            discount,
            2,
            MidpointRounding.AwayFromZero);
    }




    public async Task<Dictionary<int, CampaignDiscountResult>>
        CalculateDiscountsBatchAsync(
            IEnumerable<Product> products,
            IReadOnlyDictionary<int, decimal> basePricesByProductId)
    {
        var productList = products.ToList();
        var result = new Dictionary<int, CampaignDiscountResult>();

        if (productList.Count == 0)
            return result;

        var lookupInput = productList
            .Select(p => (ProductId: p.Id, CategoryId: p.CategoryId));

        var campaignsByProductId =
            await GetActiveCampaignsForProductsAsync(lookupInput);

        foreach (var product in productList)
        {
            var basePrice =
                basePricesByProductId.TryGetValue(product.Id, out var bp)
                    ? bp
                    : product.Price;

            if (campaignsByProductId.TryGetValue(product.Id, out var campaign))
            {
                result[product.Id] = new CampaignDiscountResult
                {
                    DiscountAmount = ComputeDiscount(campaign, basePrice),
                    CampaignName = campaign.Name
                };
            }
            else
            {
                result[product.Id] = new CampaignDiscountResult
                {
                    DiscountAmount = 0m,
                    CampaignName = null
                };
            }
        }

        return result;
    }


    // VALIDATION


    private static void ValidateCampaign(
        string name,
        int discountType,
        decimal discountValue,
        DateTime startDate,
        DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Campaign name is required.");
        }

        if (discountType !=
                (int)CampaignDiscountType.Percentage &&
            discountType !=
                (int)CampaignDiscountType.FixedAmount)
        {
            throw new ArgumentException(
                "Invalid campaign discount type.");
        }

        if (discountValue <= 0)
        {
            throw new ArgumentException(
                "Discount value must be greater than zero.");
        }

        if (discountType ==
                (int)CampaignDiscountType.Percentage &&
            discountValue > 100)
        {
            throw new ArgumentException(
                "Percentage discount cannot exceed 100%.");
        }

        if (endDate <= startDate)
        {
            throw new ArgumentException(
                "Campaign end date must be after start date.");
        }
    }


    // UTC NORMALIZATION


    private static DateTime NormalizeToUtc(
        DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc =>
                value,

            DateTimeKind.Local =>
                value.ToUniversalTime(),

            _ =>
                DateTime.SpecifyKind(
                    value,
                    DateTimeKind.Utc)
        };
    }
}



public class CampaignDiscountResult
{
    public decimal DiscountAmount { get; set; }
    public string? CampaignName { get; set; }
}