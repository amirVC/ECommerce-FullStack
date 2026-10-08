using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Http;

namespace ECommerce.Web.Services;

public class RecentlyViewedService : IRecentlyViewedService
{
    private readonly IApiClient _apiClient;

    public RecentlyViewedService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task AddAsync(int productId)
    {
        await _apiClient.PostAsync(
            "api/recently-viewed",
            new CreateRecentlyViewedDto
            {
                ProductId = productId
            });
    }

    public async Task<List<RecentlyViewedDto>> GetAsync()
    {
        return await _apiClient.GetAsync<List<RecentlyViewedDto>>("api/recently-viewed")
               ?? new();
    }

    public async Task ClearAsync()
    {
        await _apiClient.DeleteAsync<object>("api/recently-viewed");
    }
}