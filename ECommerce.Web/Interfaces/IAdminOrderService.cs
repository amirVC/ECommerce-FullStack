using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IAdminOrderService
{
    Task<List<AdminOrderDto>> GetAllAsync();

    Task<AdminOrderDetailsDto?> GetByIdAsync(int id);

    Task UpdateStatusAsync(
        int id,
        AdminUpdateOrderStatusDto dto);
}