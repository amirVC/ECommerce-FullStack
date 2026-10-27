using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IAdminCategoryService
{
    Task<IEnumerable<AdminCategoryResponseDto>> GetAllAsync();

    Task<AdminCategoryResponseDto?> GetBySlugAsync(string slug);

    Task<AdminCategoryResponseDto> CreateAsync(
        AdminCreateCategoryDto dto);

    Task<AdminCategoryResponseDto?> UpdateAsync(
        string slug,
        AdminUpdateCategoryDto dto);

    Task DeleteAsync(int id);
}