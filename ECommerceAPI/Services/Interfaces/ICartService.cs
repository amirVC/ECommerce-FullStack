using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task<CartDto> AddItemAsync(int userId, AddToCartDto dto);
    Task<CartDto> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemDto dto);
    Task<CartDto> RemoveItemAsync(int userId, int cartItemId);
    Task<CartDto> ClearCartAsync(int userId);

    Task<CartDto> ApplyCouponAsync(int userId, string code);
    Task<CartDto> RemoveCouponAsync(int userId);
}