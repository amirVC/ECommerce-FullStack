namespace ECommerce.Web.DTOs;

public class DashboardResponseDto
{
    public AdminDashboardDto Statistics { get; set; } = new();

    public IEnumerable<AdminRecentOrderDto> RecentOrders { get; set; }
        = Enumerable.Empty<AdminRecentOrderDto>();

    public IEnumerable<AdminLowStockProductDto> LowStockProducts { get; set; }
        = Enumerable.Empty<AdminLowStockProductDto>();

    public IEnumerable<AdminTopSellingProductDto> TopSellingProducts { get; set; }
        = Enumerable.Empty<AdminTopSellingProductDto>();

    public IEnumerable<MonthlySalesDto> MonthlySales { get; set; }
        = Enumerable.Empty<MonthlySalesDto>();

    public List<ActivityDto> RecentActivities { get; set; } = new();
}