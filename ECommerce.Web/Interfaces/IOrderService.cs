using ECommerce.Web.DTOs;
namespace ECommerce.Web.Interfaces;
public interface IOrderService
{
    Task<OrderResponseDto?> CheckoutAsync(CheckoutDto dto);
    Task<List<OrderResponseDto>> GetMyOrdersAsync();
    Task<OrderResponseDto?> GetOrderByIdAsync(int id);
}
