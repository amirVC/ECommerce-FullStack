using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IAdminCategoryService
{
    Task<List<AdminCategoryDto>> GetAllAsync();

    Task<AdminCategoryDto> CreateAsync(AdminCreateCategoryDto dto);

    Task<AdminCategoryDto?> GetBySlugAsync(string slug);

    Task<AdminCategoryDto> UpdateAsync(
        string slug,
        AdminUpdateCategoryDto dto);

    Task DeleteAsync(int id);
}