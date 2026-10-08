using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
namespace ECommerceAPI.Services;

public class AdminUserService : IAdminUserService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AdminUserService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<AdminUserDto>> GetAllAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                OrdersCount = u.Orders.Count,
                TotalSpent = u.Orders.Sum(o => o.TotalAmount)
            })
            .ToListAsync();
    }

    public async Task<AdminUserDetailsDto?> GetByIdAsync(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Orders)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return null;

        return new AdminUserDetailsDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            OrdersCount = user.Orders.Count,
            TotalSpent = user.Orders.Sum(o => o.TotalAmount),

            Orders = user.Orders
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new AdminUserOrderDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount
                })
                .ToList()
        };
    }

    public async Task UpdateRoleAsync(
    int id,
    UpdateUserRoleDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            throw new NotFoundException("User not found.");

        var role = dto.Role.Trim();

        if (role != "Admin" && role != "Customer")
            throw new BadRequestException("Invalid role.");

        user.Role = role;

        await _context.SaveChangesAsync();
    }



    public async Task DeleteAsync(int id)
    {
        var currentUserId = int.Parse(
    _httpContextAccessor.HttpContext!
        .User
        .FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (currentUserId == id)
        {
            throw new BadRequestException(
                "You cannot delete your own account.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            throw new NotFoundException("User not found.");

        if (user.Role == "Admin")
        {
            var adminCount = await _context.Users
                .CountAsync(u => u.Role == "Admin");

            if (adminCount <= 1)
            {
                throw new BadRequestException(
                    "The last administrator cannot be deleted.");
            }
        }

        var hasOrders = await _context.Orders.AnyAsync(o => o.UserId == id);
        if (hasOrders)
            throw new BadRequestException(
                "Cannot delete a user who has orders.");

        _context.Users.Remove(user);

        await _context.SaveChangesAsync();
    }
}