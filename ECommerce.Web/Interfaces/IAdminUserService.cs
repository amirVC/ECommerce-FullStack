using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IAdminUserService
{
    Task<List<AdminUserDto>> GetAllAsync();

    Task<AdminUserDetailsDto?> GetByIdAsync(int id);

    Task UpdateRoleAsync(int id, UpdateUserRoleDto dto);

    Task DeleteAsync(int id);
}