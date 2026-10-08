using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Extensions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services;

public class CouponService : ICouponService
{
    private readonly AppDbContext _context;
    private readonly ICampaignService _campaignService;

    public CouponService(AppDbContext context, ICampaignService campaignService)
    {
        _context = context;
        _campaignService = campaignService;
    }

    public async Task<PagedResultDto<CouponDto>> GetAllAsync(int page, int pageSize)
    {
        var query = _context.Coupons
            .AsNoTracking()
            .Include(c => c.CouponProducts)
            .Include(c => c.CouponCategories)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await query.CountAsync();

        var coupons = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<CouponDto>
        {
            Items = coupons.Select(MapToDto).ToList(),
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CouponDto?> GetByIdAsync(int id)
    {
        var coupon = await _context.Coupons
            .AsNoTracking()
            .Include(c => c.CouponProducts)
            .Include(c => c.CouponCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        return coupon is null ? null : MapToDto(coupon);
    }

    public async Task<CouponDto> CreateAsync(AdminCreateCouponDto dto)
    {
        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        var exists = await _context.Coupons.AnyAsync(c => c.Code == normalizedCode);
        if (exists)
            throw new BadRequestException("A coupon with this code already exists.");

        ValidateCouponRules(dto.DiscountType, dto.DiscountValue, dto.Scope,
            dto.ProductIds, dto.CategoryIds, dto.StartDate, dto.EndDate);

        var coupon = new Coupon
        {
            Code = normalizedCode,
            Description = dto.Description,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            Scope = dto.Scope,
            MinimumOrderAmount = dto.MinimumOrderAmount,
            UsageLimitTotal = dto.UsageLimitTotal,
            UsageLimitPerUser = dto.UsageLimitPerUser,
            StartDate = NormalizeToUtc(dto.StartDate),
            EndDate = NormalizeToUtc(dto.EndDate),
            IsActive = dto.IsActive,
            CouponProducts = dto.ProductIds.Distinct()
                .Select(pid => new CouponProduct { ProductId = pid }).ToList(),
            CouponCategories = dto.CategoryIds.Distinct()
                .Select(cid => new CouponCategory { CategoryId = cid }).ToList()
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        return MapToDto(coupon);
    }

    public async Task<CouponDto?> UpdateAsync(int id, AdminUpdateCouponDto dto)
    {
        var coupon = await _context.Coupons
            .Include(c => c.CouponProducts)
            .Include(c => c.CouponCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (coupon is null) return null;

        ValidateCouponRules(dto.DiscountType, dto.DiscountValue, dto.Scope,
            dto.ProductIds, dto.CategoryIds, dto.StartDate, dto.EndDate);

        coupon.Description = dto.Description;
        coupon.DiscountType = dto.DiscountType;
        coupon.DiscountValue = dto.DiscountValue;
        coupon.MaxDiscountAmount = dto.MaxDiscountAmount;
        coupon.Scope = dto.Scope;
        coupon.MinimumOrderAmount = dto.MinimumOrderAmount;
        coupon.UsageLimitTotal = dto.UsageLimitTotal;
        coupon.UsageLimitPerUser = dto.UsageLimitPerUser;
        coupon.StartDate = NormalizeToUtc(dto.StartDate);
        coupon.EndDate = NormalizeToUtc(dto.EndDate);
        coupon.IsActive = dto.IsActive;

        _context.CouponProducts.RemoveRange(coupon.CouponProducts);
        _context.CouponCategories.RemoveRange(coupon.CouponCategories);
        coupon.CouponProducts = dto.ProductIds.Distinct()
            .Select(pid => new CouponProduct { CouponId = id, ProductId = pid }).ToList();
        coupon.CouponCategories = dto.CategoryIds.Distinct()
            .Select(cid => new CouponCategory { CouponId = id, CategoryId = cid }).ToList();

        await _context.SaveChangesAsync();

        return MapToDto(coupon);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon is null) return false;

        _context.Coupons.Remove(coupon);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CouponValidationResultDto> ValidateAsync(string code, int userId, Cart cart)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        var coupon = await _context.Coupons
            .AsNoTracking()
            .Include(c => c.CouponProducts)
            .Include(c => c.CouponCategories)
            .FirstOrDefaultAsync(c => c.Code == normalizedCode);

        if (coupon is null)
            return Invalid("Coupon code not found.");

        var now = DateTime.UtcNow;

        if (!coupon.IsActive)
            return Invalid("This coupon is no longer active.");

        if (coupon.StartDate > now)
            return Invalid("This coupon is not active yet.");

        if (coupon.EndDate.HasValue && coupon.EndDate < now)
            return Invalid("This coupon has expired.");

        if (coupon.UsageLimitTotal.HasValue && coupon.TimesUsed >= coupon.UsageLimitTotal)
            return Invalid("This coupon has reached its usage limit.");

        if (coupon.UsageLimitPerUser.HasValue)
        {
            var userUsageCount = await _context.CouponUsages
                .CountAsync(u => u.CouponId == coupon.Id && u.UserId == userId);
            if (userUsageCount >= coupon.UsageLimitPerUser)
                return Invalid("You've already used this coupon the maximum number of times.");
        }

        if (!cart.CartItems.Any())
            return Invalid("Your cart is empty.");


        var campaignKeys = cart.CartItems
            .Select(i => (ProductId: i.Product.Id, CategoryId: i.Product.CategoryId))
            .ToList();

        var activeCampaigns = await _campaignService.GetActiveCampaignsForProductsAsync(campaignKeys);

        var postCampaignPrices = new Dictionary<int, decimal>(); // CartItem.Id -> unit price

        decimal cartTotal = 0m;
        foreach (var item in cart.CartItems)
        {
            var salePrice = item.Product.EffectivePrice();

            activeCampaigns.TryGetValue(item.ProductId, out var campaign);
            var campaignDiscount = CampaignService.ComputeDiscount(campaign, salePrice);

            var postCampaignPrice = Math.Max(0m, salePrice - campaignDiscount);

            postCampaignPrices[item.Id] = postCampaignPrice;
            cartTotal += postCampaignPrice * item.Quantity;
        }

        if (coupon.MinimumOrderAmount.HasValue && cartTotal < coupon.MinimumOrderAmount)
            return Invalid($"This coupon requires a minimum order of {coupon.MinimumOrderAmount:C}.");

        var eligibleItems = coupon.Scope switch
        {
            CouponScope.SpecificProducts => cart.CartItems
                .Where(i => coupon.CouponProducts.Any(cp => cp.ProductId == i.ProductId)),
            CouponScope.SpecificCategories => cart.CartItems
                .Where(i => coupon.CouponCategories.Any(cc => cc.CategoryId == i.Product.CategoryId)),
            _ => cart.CartItems.AsEnumerable()
        };

        var eligibleTotal = eligibleItems.Sum(i => postCampaignPrices[i.Id] * i.Quantity);

        if (eligibleTotal <= 0)
            return Invalid("This coupon doesn't apply to any items in your cart.");

        decimal discountAmount = coupon.DiscountType switch
        {
            CouponDiscountType.Percentage => eligibleTotal * (coupon.DiscountValue ?? 0) / 100m,
            CouponDiscountType.FixedAmount => Math.Min(coupon.DiscountValue ?? 0, eligibleTotal),
            CouponDiscountType.FreeShipping => 0m,
            _ => 0m
        };

        if (coupon.MaxDiscountAmount.HasValue)
            discountAmount = Math.Min(discountAmount, coupon.MaxDiscountAmount.Value);

        return new CouponValidationResultDto
        {
            IsValid = true,
            Code = coupon.Code,
            DiscountType = coupon.DiscountType.ToString(),
            DiscountAmount = Math.Round(discountAmount, 2),
            FreeShipping = coupon.DiscountType == CouponDiscountType.FreeShipping
        };
    }

    public async Task RecordUsageAsync(string code, int userId, int orderId, decimal discountApplied)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == code.Trim().ToUpperInvariant());
        if (coupon is null) return;

        coupon.TimesUsed++;
        _context.CouponUsages.Add(new CouponUsage
        {
            CouponId = coupon.Id,
            UserId = userId,
            OrderId = orderId,
            DiscountAmountApplied = discountApplied
        });

        await _context.SaveChangesAsync();
    }

    private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static DateTime? NormalizeToUtc(DateTime? value) =>
        value.HasValue ? NormalizeToUtc(value.Value) : null;

    private static void ValidateCouponRules(
        CouponDiscountType discountType,
        decimal? discountValue,
        CouponScope scope,
        List<int> productIds,
        List<int> categoryIds,
        DateTime startDate,
        DateTime? endDate)
    {
        if (discountType == CouponDiscountType.Percentage)
        {
            if (!discountValue.HasValue || discountValue <= 0 || discountValue > 100)
                throw new BadRequestException("Percentage discount must be between 0 and 100.");
        }
        else if (discountType == CouponDiscountType.FixedAmount)
        {
            if (!discountValue.HasValue || discountValue <= 0)
                throw new BadRequestException("Fixed discount amount must be greater than 0.");
        }
        // FreeShipping: DiscountValue is ignored/not required.

        if (scope == CouponScope.SpecificProducts && !productIds.Any())
            throw new BadRequestException("Select at least one product for a product-specific coupon.");

        if (scope == CouponScope.SpecificCategories && !categoryIds.Any())
            throw new BadRequestException("Select at least one category for a category-specific coupon.");

        if (endDate.HasValue && endDate.Value <= startDate)
            throw new BadRequestException("End date must be after the start date.");
    }

    private static CouponValidationResultDto Invalid(string message) =>
        new() { IsValid = false, ErrorMessage = message };

    private static CouponDto MapToDto(Coupon c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        Description = c.Description,
        DiscountType = c.DiscountType.ToString(),
        DiscountValue = c.DiscountValue,
        MaxDiscountAmount = c.MaxDiscountAmount,
        Scope = c.Scope.ToString(),
        MinimumOrderAmount = c.MinimumOrderAmount,
        UsageLimitTotal = c.UsageLimitTotal,
        UsageLimitPerUser = c.UsageLimitPerUser,
        TimesUsed = c.TimesUsed,
        StartDate = c.StartDate,
        EndDate = c.EndDate,
        IsActive = c.IsActive,
        ProductIds = c.CouponProducts.Select(cp => cp.ProductId).ToList(),
        CategoryIds = c.CouponCategories.Select(cc => cc.CategoryId).ToList()
    };
}