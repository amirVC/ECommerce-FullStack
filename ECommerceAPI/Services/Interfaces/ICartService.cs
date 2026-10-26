using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);

    Task<CartDto> AddItemAsync(int userId, AddToCartDto dto);

    Task<CartDto> UpdateItemAsync(int userId, Guid publicId, UpdateCartItemDto dto);

    Task<CartDto> RemoveItemAsync(int userId, Guid publicId);

    Task<CartDto> ClearCartAsync(int userId);

    Task IncreaseQuantityAsync(Guid publicId, int userId);

    Task DecreaseQuantityAsync(Guid publicId, int userId);

    Task<CartDto> ApplyCouponAsync(int userId, string code);

    Task<CartDto> RemoveCouponAsync(int userId);
}