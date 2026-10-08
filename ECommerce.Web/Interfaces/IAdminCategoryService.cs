using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IAdminCategoryService
{
    Task<List<AdminCategoryDto>> GetAllAsync();

    Task<AdminCategoryDto> CreateAsync(AdminCreateCategoryDto dto);

    Task<AdminCategoryDto?> GetByIdAsync(int id);

    Task<AdminCategoryDto> UpdateAsync(
        int id,
        AdminUpdateCategoryDto dto);

    Task DeleteAsync(int id);
}