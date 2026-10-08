using ECommerce.Web.DTOs;
using ECommerce.Web.ViewModels;

namespace ECommerce.Web.Interfaces;

public interface IAdminProductService
{
    Task<List<AdminProductDto>> GetAllAsync();

    Task<AdminProductDto> CreateAsync(AdminCreateProductDto dto, IFormFile? imageFile);

    Task<AdminProductDto> UpdateAsync(int id, AdminUpdateProductDto dto, IFormFile? imageFile);

    Task DeleteAsync(int id);

    Task<AdminUpdateProductDto?> GetByIdAsync(int id);

    Task<PagedResultDto<AdminProductDto>> SearchAsync(ProductSearchDto dto);

    Task<List<ProductImageViewModel>> GetImagesAsync(int productId);

    Task AddImageAsync(
        int productId,
        AdminCreateProductImageViewModel model);

    Task DeleteImageAsync(int imageId);

    Task SetPrimaryImageAsync(int imageId);

    // Added method to handle the inline alt text update
    Task UpdateImageAltTextAsync(int imageId, string altText);
}