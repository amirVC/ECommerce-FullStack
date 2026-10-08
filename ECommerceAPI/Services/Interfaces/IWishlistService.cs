using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IWishlistService
{
    Task<WishlistDto> GetWishlistAsync(int userId);
    Task<WishlistDto> AddItemAsync(int userId, AddToWishlistDto dto);
    Task<WishlistDto> RemoveItemAsync(int userId, int wishlistItemId);
    Task<WishlistDto> RemoveByProductAsync(int userId, int productId);
    Task<WishlistDto> ClearWishlistAsync(int userId);
}