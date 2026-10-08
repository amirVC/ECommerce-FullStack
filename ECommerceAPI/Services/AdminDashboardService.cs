using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerceAPI.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    private const string DashboardDataCacheKey = "admin_dashboard_data";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(3);

    private const int LowStockThreshold = 5;
    private const int RecentItemsLimit = 10;
    private const int ActivityItemsLimit = 10;

    public AdminDashboardService(
        AppDbContext context,
        IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }




    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var totalProducts = await _context.Products
            .AsNoTracking()
            .CountAsync();

        var totalCategories = await _context.Categories
            .AsNoTracking()
            .CountAsync();

        var totalUsers = await _context.Users
            .AsNoTracking()
            .CountAsync();

        var orderStats = await _context.Orders
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalOrders = g.Count(),

                TotalRevenue =
                    g.Where(o => o.Status == "Delivered")
                        .Sum(o => (decimal?)o.TotalAmount) ?? 0m,

                PendingOrders =
                    g.Count(o => o.Status == "Pending"),

                DeliveredOrders =
                    g.Count(o => o.Status == "Delivered"),

                CancelledOrders =
                    g.Count(o => o.Status == "Cancelled")
            })
            .FirstOrDefaultAsync();

        return new AdminDashboardDto
        {
            TotalProducts = totalProducts,

            TotalCategories = totalCategories,

            TotalUsers = totalUsers,

            TotalOrders = orderStats?.TotalOrders ?? 0,

            TotalRevenue = orderStats?.TotalRevenue ?? 0m,

            PendingOrders = orderStats?.PendingOrders ?? 0,

            DeliveredOrders = orderStats?.DeliveredOrders ?? 0,

            CancelledOrders = orderStats?.CancelledOrders ?? 0
        };
    }




    public async Task<IEnumerable<AdminRecentOrderDto>> GetRecentOrdersAsync()
    {
        return await _context.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .Take(RecentItemsLimit)
            .Select(o => new AdminRecentOrderDto
            {
                Id = o.Id,

                CustomerName = o.User.FullName,

                TotalAmount = o.TotalAmount,

                Status = o.Status,

                OrderDate = o.OrderDate
            })
            .ToListAsync();
    }




    public async Task<IEnumerable<AdminLowStockProductDto>> GetLowStockProductsAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.Stock <= LowStockThreshold)
            .OrderBy(p => p.Stock)
            .ThenBy(p => p.Id)
            .Select(p => new AdminLowStockProductDto
            {
                Id = p.Id,

                Name = p.Name,

                Stock = p.Stock,

                CategoryName = p.Category.Name
            })
            .ToListAsync();
    }




    public async Task<IEnumerable<AdminTopSellingProductDto>> GetTopSellingProductsAsync()
    {
        return await _context.OrderItems
            .AsNoTracking()
            .GroupBy(oi => new
            {
                oi.ProductId,
                ProductName = oi.Product.Name
            })
            .Select(g => new AdminTopSellingProductDto
            {
                ProductId = g.Key.ProductId,

                ProductName = g.Key.ProductName,

                QuantitySold = g.Sum(x => x.Quantity),

                Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.QuantitySold)
            .ThenBy(x => x.ProductId)
            .Take(RecentItemsLimit)
            .ToListAsync();
    }




    public async Task<IEnumerable<MonthlySalesDto>> GetMonthlySalesAsync()
    {
        var oneYearAgo = DateTime.UtcNow.AddMonths(-12);

        return await _context.Orders
            .AsNoTracking()
            .Where(o =>
                o.OrderDate >= oneYearAgo &&
                o.Status == "Delivered")
            .GroupBy(o => new
            {
                o.OrderDate.Year,
                o.OrderDate.Month
            })
            .Select(g => new MonthlySalesDto
            {
                Year = g.Key.Year,

                Month = g.Key.Month,

                MonthName = new DateTime(
                    g.Key.Year,
                    g.Key.Month,
                    1)
                    .ToString("MMMM"),

                OrdersCount = g.Count(),

                Revenue = g.Sum(x => x.TotalAmount)
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();
    }


    // COMPLETE DASHBOARD


    public async Task<DashboardResponseDto> GetDashboardDataAsync()
    {
        if (_cache.TryGetValue(
                DashboardDataCacheKey,
                out DashboardResponseDto? cached))
        {
            return cached!;
        }

        var data = new DashboardResponseDto
        {
            Statistics = await GetDashboardAsync(),

            RecentOrders = await GetRecentOrdersAsync(),

            LowStockProducts = await GetLowStockProductsAsync(),

            TopSellingProducts = await GetTopSellingProductsAsync(),

            MonthlySales = await GetMonthlySalesAsync(),

            RecentActivities = await GetRecentActivitiesAsync()
        };

        _cache.Set(
            DashboardDataCacheKey,
            data,
            CacheDuration);

        return data;
    }


    // RECENT ACTIVITIES


    public async Task<List<ActivityDto>> GetRecentActivitiesAsync()
    {
        var activities = new List<ActivityDto>();


        // Recent Orders


        var recentOrders = await _context.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .Select(o => new
            {
                o.Id,
                o.OrderDate
            })
            .ToListAsync();

        foreach (var order in recentOrders)
        {
            activities.Add(new ActivityDto
            {
                Icon = "bi-cart-check-fill",

                Title = $"Order #{order.Id}",

                Description = "New order has been placed.",

                Date = order.OrderDate
            });
        }


        // Recent Users


        var recentUsers = await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new
            {
                u.FullName,
                u.CreatedAt
            })
            .ToListAsync();

        foreach (var user in recentUsers)
        {
            activities.Add(new ActivityDto
            {
                Icon = "bi-person-plus-fill",

                Title = user.FullName,

                Description = "New customer registered.",

                Date = user.CreatedAt
            });
        }


        // Recent Products


        var recentProducts = await _context.Products
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new
            {
                p.Name,
                p.CreatedAt
            })
            .ToListAsync();

        foreach (var product in recentProducts)
        {
            activities.Add(new ActivityDto
            {
                Icon = "bi-box-seam-fill",

                Title = product.Name,

                Description = "New product added.",

                Date = product.CreatedAt
            });
        }




        var lowStockProducts = await _context.Products
            .AsNoTracking()
            .Where(p => p.Stock <= LowStockThreshold)
            .OrderBy(p => p.Stock)
            .ThenBy(p => p.Id)
            .Take(5)
            .Select(p => new
            {
                p.Name,
                p.Stock,
                p.CreatedAt
            })
            .ToListAsync();

        foreach (var product in lowStockProducts)
        {
            activities.Add(new ActivityDto
            {
                Icon = "bi-exclamation-triangle-fill",

                Title = product.Name,

                Description = $"Only {product.Stock} left in stock.",

                Date = product.CreatedAt
            });
        }

        return activities
            .OrderByDescending(a => a.Date)
            .Take(ActivityItemsLimit)
            .ToList();
    }
}
