using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IAdminDashboardService
{
    Task<DashboardResponseDto?> GetDashboardAsync();
}