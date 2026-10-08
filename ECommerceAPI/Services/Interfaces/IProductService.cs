using ECommerceAPI.Common;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models.QueryParameters;

namespace ECommerceAPI.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductResponseDto>> GetAllAsync(ProductQueryParameters query);

    Task<ProductResponseDto?> GetByIdAsync(int id);

    Task<ProductResponseDto?> GetBySkuAsync(string sku);

    Task<IEnumerable<ProductResponseDto>> GetByCategoryAsync(int categoryId);

    Task<ProductResponseDto> CreateAsync(ProductDto dto);

    Task<ProductResponseDto?> UpdateAsync(int id, ProductDto dto);

    Task<bool> DeleteAsync(int id);

    Task<AdminUpdateProductDto?> GetAdminByIdAsync(int id);

    Task<List<ProductResponseDto>> GetRelatedAsync(int id, int take = 4);
}
