using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _dashboardService;

    public AdminDashboardController(
        IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard =
            await _dashboardService.GetDashboardDataAsync();

        return Ok(dashboard);
    }

    [HttpGet("recent-orders")]
    public async Task<IActionResult> GetRecentOrders()
    {
        var orders =
            await _dashboardService.GetRecentOrdersAsync();

        return Ok(orders);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts()
    {
        var products =
            await _dashboardService.GetLowStockProductsAsync();

        return Ok(products);
    }

    [HttpGet("top-selling-products")]
    public async Task<IActionResult> GetTopSellingProducts()
    {
        var products =
            await _dashboardService.GetTopSellingProductsAsync();

        return Ok(products);
    }

    [HttpGet("monthly-sales")]
    public async Task<IActionResult> GetMonthlySales()
    {
        var sales =
            await _dashboardService.GetMonthlySalesAsync();

        return Ok(sales);
    }

    [HttpGet("recent-activities")]
    public async Task<IActionResult> GetRecentActivities()
    {
        var activities =
            await _dashboardService.GetRecentActivitiesAsync();

        return Ok(activities);
    }
}