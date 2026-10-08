
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

        return await DeserializeAsync<List<AdminOrderDto>>(response)
               ?? new();
    }

    public async Task<AdminOrderDetailsDto?> GetByIdAsync(int id)
    {
        var client = CreateClient();

        var response = await client.GetAsync($"api/admin/orders/{id}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeAsync<AdminOrderDetailsDto>(response);
    }

    public async Task UpdateStatusAsync(
        int id,
        AdminUpdateOrderStatusDto dto)
    {
        var client = CreateClient();

        var response = await client.PutAsync(
            $"api/admin/orders/{id}/status",
            new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                "application/json"));

        response.EnsureSuccessStatusCode();
    }
}

