using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IAdminOrderService
{
    Task<PagedResultDto<AdminOrderDto>> GetAllAsync(int page = 1, int pageSize = 20);

    Task<AdminOrderDetailsDto?> GetByIdAsync(int id);

    Task UpdateStatusAsync(
        int id,
        AdminUpdateOrderStatusDto dto);
}