using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Http;
using ECommerce.Web.ViewModels;

namespace ECommerce.Web.Services;

public class WishlistService : IWishlistService
{
    private const string WishlistApiBase = "api/wishlist";

    private readonly IApiClient _apiClient;

    public WishlistService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<WishlistItemViewModel>> GetWishlistAsync()
    {
        var wishlist =
            await _apiClient.GetAsync<WishlistDto>(WishlistApiBase);

        return MapFromApi(wishlist);
    }

    public async Task AddToWishlistAsync(int productId)
    {
        await _apiClient.PostAsync<AddToWishlistDto, WishlistDto>(
            $"{WishlistApiBase}/items",
            new AddToWishlistDto
            {
                ProductId = productId
            });
    }

    public async Task RemoveFromWishlistAsync(int wishlistItemId)
    {
        await _apiClient.DeleteAsync<WishlistDto>(
            $"{WishlistApiBase}/items/{wishlistItemId}");
    }

    public async Task RemoveByProductAsync(int productId)
    {
        await _apiClient.DeleteAsync<WishlistDto>(
            $"{WishlistApiBase}/products/{productId}");
    }

    public async Task ClearWishlistAsync()
    {
        await _apiClient.DeleteAsync<WishlistDto>(
            WishlistApiBase);
    }

    public async Task<int> GetWishlistCountAsync()
    {
        var wishlist = await GetWishlistAsync();

        return wishlist.Count;
    }

    public async Task<HashSet<int>> GetWishlistedProductIdsAsync()
    {
        var wishlist = await GetWishlistAsync();

        return wishlist
            .Select(i => i.ProductId)
            .ToHashSet();
    }

    // ---------- Helpers ----------

    private static List<WishlistItemViewModel> MapFromApi(
        WishlistDto? wishlist)
    {
        if (wishlist == null)
            return new List<WishlistItemViewModel>();

        return wishlist.Items
            .Select(i => new WishlistItemViewModel
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ImageUrl = i.ProductImageUrl,
                ThumbnailUrl = i.ProductThumbnailUrl,  
                Price = i.Price,
                FinalPrice = i.FinalPrice,
                SaleDiscountAmount = i.SaleDiscountAmount,
                CampaignDiscountAmount = i.CampaignDiscountAmount,
                TotalDiscountAmount = i.TotalDiscountAmount,
                TotalDiscountPercentage = i.TotalDiscountPercentage,
                IsOnSale = i.IsOnSale,
                HasCampaign = i.HasCampaign,
                CampaignName = i.CampaignName,
                AvailableStock = i.AvailableStock,
                InStock = i.InStock,
                AddedAt = i.AddedAt
            })
            .ToList();       
    }
}