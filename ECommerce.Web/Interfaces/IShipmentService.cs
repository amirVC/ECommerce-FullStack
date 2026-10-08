using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IShipmentService
{
    // Customer + admin — order tracking view
    Task<ShipmentDto?> GetByOrderIdAsync(int orderId);

    // Admin
    Task<ShipmentDto?> CreateForOrderAsync(int orderId, CreateShipmentDto dto);
    Task<ShipmentDto?> UpdateStatusAsync(int orderId, UpdateShipmentStatusDto dto);
}
