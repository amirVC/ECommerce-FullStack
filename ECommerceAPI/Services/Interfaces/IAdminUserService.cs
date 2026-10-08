using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IAdminUserService
{
    Task<List<AdminUserDto>> GetAllAsync();

    Task<AdminUserDetailsDto?> GetByIdAsync(int id);

    Task UpdateRoleAsync(int id, UpdateUserRoleDto dto);

    Task DeleteAsync(int id);
}