using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Base;

namespace ECommerce.Web.Services;
public class AdminDashboardService
    : ApiServiceBase,
      IAdminDashboardService
{
    public AdminDashboardService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, httpContextAccessor)
    {
    }

    public async Task<DashboardResponseDto?> GetDashboardAsync()
    {
        var client = CreateClient();

        var response = await client.GetAsync("api/admin/dashboard");

        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeAsync<DashboardResponseDto>(response);
    }
}