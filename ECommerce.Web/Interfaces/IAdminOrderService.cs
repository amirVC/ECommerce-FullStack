
using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IAdminOrderService
{
    Task<List<AdminOrderDto>> GetAllAsync();

    Task<AdminOrderDetailsDto?> GetByIdAsync(Guid publicId);

    Task UpdateStatusAsync(
        Guid publicId,
        AdminUpdateOrderStatusDto dto);
}
