namespace ECommerce.Web.DTOs;

public class DashboardResponseDto
{
    public AdminDashboardDto Statistics { get; set; } = new();

    public List<AdminRecentOrderDto> RecentOrders { get; set; } = new();

    public List<AdminLowStockProductDto> LowStockProducts { get; set; } = new();

    public List<AdminTopSellingProductDto> TopSellingProducts { get; set; } = new();

    public List<MonthlySalesDto> MonthlySales { get; set; } = new();

    public List<ActivityDto> RecentActivities { get; set; } = new();
}