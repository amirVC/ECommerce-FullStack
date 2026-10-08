using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IProductImageService
{
    Task<List<ProductImageDto>> GetImagesAsync(int productId);

    Task<ProductImageDto> AddImageAsync(int productId, AdminCreateProductImageDto dto);
    Task DeleteImageAsync(int imageId);

    Task SetPrimaryImageAsync(int imageId);

    Task UpdateAltTextAsync(int imageId, string altText);
}