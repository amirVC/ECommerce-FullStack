using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces
{
public interface IShipmentService
    {
        Task<ShipmentDto?> GetByOrderIdAsync(int orderId, int userId, bool isAdmin);
        Task<ShipmentDto> CreateForOrderAsync(int orderId, CreateShipmentDto dto);   // admin
        Task<ShipmentDto> UpdateStatusAsync(int orderId, UpdateShipmentStatusDto dto); // admin
    }
}
