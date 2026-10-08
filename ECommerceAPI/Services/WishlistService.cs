using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Extensions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services;

public class WishlistService : IWishlistService
{
    private readonly AppDbContext _context;
    private readonly ICampaignService _campaignService;

    public WishlistService(
        AppDbContext context,
        ICampaignService campaignService)
    {
        _context = context;
        _campaignService = campaignService;
    }

    public async Task<WishlistDto> GetWishlistAsync(int userId)
    {
        var wishlist =
            await GetOrCreateWishlistAsync(userId);

        return await MapToDtoAsync(wishlist);
    }

    public async Task<WishlistDto> AddItemAsync(
        int userId,
        AddToWishlistDto dto)
    {
        var product =
            await _context.Products.FindAsync(dto.ProductId);

        if (product == null)
        {
            throw new NotFoundException(
                $"Product {dto.ProductId} not found.");
        }

        var wishlist =
            await GetOrCreateWishlistAsync(userId);

        var alreadyExists =
            wishlist.WishlistItems.Any(
                wi => wi.ProductId == dto.ProductId);

        if (alreadyExists)
        {
            throw new BadRequestException(
                $"{product.Name} is already in your wishlist.");
        }

        wishlist.WishlistItems.Add(
            new WishlistItem
            {
                WishlistId = wishlist.Id,
                ProductId = product.Id,
                Product = product // wire up directly — avoids re-querying below
            });

        await _context.SaveChangesAsync();

        return await MapToDtoAsync(wishlist);
    }

    public async Task<WishlistDto> RemoveItemAsync(
        int userId,
        int wishlistItemId)
    {
        var item =
            await GetOwnedWishlistItemAsync(
                userId,
                wishlistItemId);

        _context.WishlistItems.Remove(item);

        var wishlist = await GetOrCreateWishlistAsync(userId);
        var toRemove = wishlist.WishlistItems.FirstOrDefault(wi => wi.Id == wishlistItemId);
        if (toRemove != null)
            wishlist.WishlistItems.Remove(toRemove);

        await _context.SaveChangesAsync();

        return await MapToDtoAsync(wishlist);
    }

    public async Task<WishlistDto> RemoveByProductAsync(
        int userId,
        int productId)
    {
        var wishlist =
            await GetOrCreateWishlistAsync(userId);

        var item =
            wishlist.WishlistItems.FirstOrDefault(
                wi => wi.ProductId == productId);

        if (item == null)
        {
            throw new NotFoundException(
                "Product not found in wishlist.");
        }

        _context.WishlistItems.Remove(item);
        wishlist.WishlistItems.Remove(item);

        await _context.SaveChangesAsync();

        return await MapToDtoAsync(wishlist);
    }

    public async Task<WishlistDto> ClearWishlistAsync(
        int userId)
    {
        var wishlist =
            await GetOrCreateWishlistAsync(userId);

        _context.WishlistItems.RemoveRange(
            wishlist.WishlistItems);
        wishlist.WishlistItems.Clear(); // keep in-memory navigation in sync with the removal above

        await _context.SaveChangesAsync();

        return await MapToDtoAsync(wishlist);
    }


    private async Task<Wishlist> GetOrCreateWishlistAsync(
        int userId)
    {
        var wishlist =
            await _context.Wishlists
                .Include(w => w.WishlistItems)
                .ThenInclude(wi => wi.Product)
                .FirstOrDefaultAsync(
                    w => w.UserId == userId);

        if (wishlist != null)
            return wishlist;

        wishlist = new Wishlist
        {
            UserId = userId
        };

        _context.Wishlists.Add(wishlist);

        await _context.SaveChangesAsync();

        return wishlist;
    }

    private async Task<WishlistItem> GetOwnedWishlistItemAsync(
        int userId,
        int wishlistItemId)
    {
        var item =
            await _context.WishlistItems
                .Include(wi => wi.Product)
                .Include(wi => wi.Wishlist)
                .FirstOrDefaultAsync(
                    wi =>
                        wi.Id == wishlistItemId &&
                        wi.Wishlist.UserId == userId);

        if (item == null)
        {
            throw new NotFoundException(
                "Wishlist item not found.");
        }

        return item;
    }

    private async Task<WishlistDto> MapToDtoAsync(
        Wishlist wishlist)
    {
        var items =
            new List<WishlistItemDto>();

        if (wishlist.WishlistItems.Count == 0)
        {
            return new WishlistDto
            {
                Id = wishlist.Id,
                Items = items,
                TotalItems = 0
            };
        }

        var campaignKeys = wishlist.WishlistItems
            .Select(wi => (
                ProductId: wi.Product.Id,
                CategoryId: wi.Product.CategoryId))
            .ToList();

        var activeCampaigns = await _campaignService
            .GetActiveCampaignsForProductsAsync(campaignKeys);

        foreach (var wishlistItem in wishlist.WishlistItems)
        {
            var product = wishlistItem.Product;

            var originalPrice = product.Price;
            var salePrice = product.EffectivePrice();
            var saleDiscount = Math.Max(0m, originalPrice - salePrice);

            activeCampaigns.TryGetValue(product.Id, out var campaign);
            var campaignDiscount = CampaignService.ComputeDiscount(campaign, salePrice);

            var finalPrice = Math.Max(0m, salePrice - campaignDiscount);
            var totalDiscount = Math.Max(0m, originalPrice - finalPrice);

            var totalDiscountPercentage = originalPrice > 0
                ? Math.Round(totalDiscount / originalPrice * 100m, 2)
                : 0m;

            items.Add(
                new WishlistItemDto
                {
                    Id = wishlistItem.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductImageUrl = product.ImageUrl,
                    ProductThumbnailUrl = product.ThumbnailUrl,

                    Price = originalPrice,
                    FinalPrice = finalPrice,
                    SaleDiscountAmount = saleDiscount,
                    CampaignDiscountAmount = campaignDiscount,
                    TotalDiscountAmount = totalDiscount,
                    TotalDiscountPercentage = totalDiscountPercentage,
                    IsOnSale = product.IsOnSale(),
                    HasCampaign = campaignDiscount > 0,
                    CampaignName = campaignDiscount > 0 ? campaign?.Name : null,
                    AvailableStock = product.Stock,
                    InStock = product.Stock > 0,
                    AddedAt = wishlistItem.AddedAt
                });
        }

        return new WishlistDto
        {
            Id = wishlist.Id,

            Items = items,

            TotalItems =
                items.Count
        };
    }
}