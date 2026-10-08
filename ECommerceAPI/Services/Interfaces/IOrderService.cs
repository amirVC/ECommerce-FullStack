using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponseDto> CheckoutAsync(
        CheckoutDto dto,
        int userId);



}