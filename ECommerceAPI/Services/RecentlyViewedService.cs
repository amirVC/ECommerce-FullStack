using AutoMapper;
using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Extensions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services;

public class RecentlyViewedService : IRecentlyViewedService
{
    private const int MaxRecentlyViewed = 20;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICampaignService _campaignService;

    public RecentlyViewedService(
        AppDbContext context,
        IMapper mapper,
        ICampaignService campaignService)
    {
        _context = context;
        _mapper = mapper;
        _campaignService = campaignService;
    }

    public async Task ClearAsync(int userId)
    {
        await _context.RecentlyViewed
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task AddAsync(int userId, int productId)
    {
        var existing = await _context.RecentlyViewed
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.ProductId == productId);

        if (existing != null)
        {
            existing.ViewedAt = DateTime.UtcNow;
        }
        else
        {
            _context.RecentlyViewed.Add(new RecentlyViewed
            {
                UserId = userId,
                ProductId = productId,
                ViewedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        var idsToKeep = await _context.RecentlyViewed
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ViewedAt)
            .Select(x => x.Id)
            .Take(MaxRecentlyViewed)
            .ToListAsync();

        await _context.RecentlyViewed
            .Where(x => x.UserId == userId && !idsToKeep.Contains(x.Id))
            .ExecuteDeleteAsync();
    }

    public async Task<List<ProductDto>> GetRecentlyViewedAsync(int userId, int count = 20)
    {
        var products = await _context.RecentlyViewed
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ViewedAt)
            .Take(count)
            .Include(x => x.Product)
                .ThenInclude(p => p.Images)
            .Select(x => x.Product)
            .ToListAsync();

        var dtos = _mapper.Map<List<ProductDto>>(products);

        if (products.Count == 0)
            return dtos;


        var campaignKeys = products
            .Select(p => (ProductId: p.Id, CategoryId: p.CategoryId))
            .ToList();

        var activeCampaigns = await _campaignService.GetActiveCampaignsForProductsAsync(campaignKeys);

        for (int i = 0; i < products.Count; i++)
        {
            var product = products[i];
            var dto = dtos[i];

            var salePrice = product.IsOnSale()
                ? product.SalePrice!.Value
                : product.Price;

            activeCampaigns.TryGetValue(product.Id, out var campaign);
            var campaignDiscount = CampaignService.ComputeDiscount(campaign, salePrice);

            dto.CampaignDiscount = campaignDiscount;
            dto.HasCampaign = campaignDiscount > 0;
            dto.CampaignName = campaignDiscount > 0 ? campaign?.Name : null;

            dto.FinalPrice = Math.Max(0m, salePrice - campaignDiscount);
            dto.TotalDiscountAmount = Math.Max(0m, product.Price - dto.FinalPrice);
        }

        return dtos;
    }
}