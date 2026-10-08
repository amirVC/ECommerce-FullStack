namespace ECommerce.Web.DTOs;
public class AdminDashboardDto
{
    public int TotalProducts { get; set; }

    public int TotalCategories { get; set; }

    public int TotalUsers { get; set; }

    public int TotalOrders { get; set; }

    public decimal TotalRevenue { get; set; }

    public int PendingOrders { get; set; }

    public int DeliveredOrders { get; set; }

    public int CancelledOrders { get; set; }
}