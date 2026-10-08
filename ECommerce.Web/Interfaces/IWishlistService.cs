using ECommerce.Web.ViewModels;

namespace ECommerce.Web.Interfaces;

public interface IWishlistService
{
    Task<List<WishlistItemViewModel>> GetWishlistAsync();
    Task AddToWishlistAsync(int productId);
    Task RemoveFromWishlistAsync(int wishlistItemId);
    Task RemoveByProductAsync(int productId);
    Task ClearWishlistAsync();
    Task<int> GetWishlistCountAsync();
    Task<HashSet<int>> GetWishlistedProductIdsAsync();
}