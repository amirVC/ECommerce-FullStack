using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IAdminProductService
{
    Task<PagedResultDto<AdminProductResponseDto>> GetAllAsync(int page = 1, int pageSize = 20);

    Task<AdminProductResponseDto> CreateAsync(AdminCreateProductDto dto);

    Task<AdminProductResponseDto> UpdateAsync(
    int id,
    AdminUpdateProductDto dto);

    Task DeleteAsync(int id);

    Task<AdminUpdateProductDto?> GetByIdAsync(int id);

    Task<AdminUpdateProductDto?> GetBySlugAsync(string slug);

    Task<AdminProductResponseDto> UpdateBySlugAsync(
        string slug,
        AdminUpdateProductDto dto);

    Task<PagedResultDto<AdminProductResponseDto>> SearchAsync(ProductSearchDto dto);
}