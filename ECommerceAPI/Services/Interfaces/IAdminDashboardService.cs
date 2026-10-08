using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardAsync();

    Task<IEnumerable<AdminRecentOrderDto>> GetRecentOrdersAsync();

    Task<IEnumerable<AdminLowStockProductDto>> GetLowStockProductsAsync();

    Task<IEnumerable<AdminTopSellingProductDto>> GetTopSellingProductsAsync();

    Task<IEnumerable<MonthlySalesDto>> GetMonthlySalesAsync();

    Task<DashboardResponseDto> GetDashboardDataAsync();

    Task<List<ActivityDto>> GetRecentActivitiesAsync();
}