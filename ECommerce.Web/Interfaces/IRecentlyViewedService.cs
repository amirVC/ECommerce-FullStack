using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IRecentlyViewedService
{
    Task AddAsync(int productId);
    Task<List<RecentlyViewedDto>> GetAsync();
    Task ClearAsync();
}