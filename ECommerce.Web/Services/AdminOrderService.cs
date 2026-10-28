
using System.Text;
using System.Text.Json;
using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Base;

namespace ECommerce.Web.Services;

public class AdminOrderService : ApiServiceBase, IAdminOrderService
{
    public AdminOrderService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, httpContextAccessor)
    {
    }

    public async Task<List<AdminOrderDto>> GetAllAsync()
    {
        var client = CreateClient();

        var response = await client.GetAsync("api/admin/orders");

        response.EnsureSuccessStatusCode();

        var result = await DeserializeAsync<PagedResultDto<AdminOrderDto>>(
            response);

        return result?.Items ?? new();
    }


    public async Task<AdminOrderDetailsDto?> GetByIdAsync(Guid publicId)
    {
        var client = CreateClient();
        var response = await client.GetAsync(
            $"api/admin/orders/{publicId}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeAsync<AdminOrderDetailsDto>(response);
    }


    public async Task UpdateStatusAsync(
        Guid publicId,
        AdminUpdateOrderStatusDto dto)
    {
        var client = CreateClient();

        var response = await client.PutAsync(
            $"api/admin/orders/{publicId}/status",
            new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                "application/json"));

        response.EnsureSuccessStatusCode();
    }
}

