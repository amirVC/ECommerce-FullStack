using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IRecentlyViewedService
{
    Task AddAsync(int userId, int productId);

    Task<List<ProductDto>> GetRecentlyViewedAsync(int userId, int count = 20);

    Task ClearAsync(int userId);
}