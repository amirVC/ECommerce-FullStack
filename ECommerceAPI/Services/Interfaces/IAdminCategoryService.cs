using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IAdminCategoryService
{
    Task<IEnumerable<AdminCategoryResponseDto>> GetAllAsync();

    Task<AdminCategoryResponseDto?> GetByIdAsync(int id);

    Task<AdminCategoryResponseDto> CreateAsync(
        AdminCreateCategoryDto dto);

    Task<AdminCategoryResponseDto> UpdateAsync(
        int id,
        AdminUpdateCategoryDto dto);

    Task DeleteAsync(int id);


}