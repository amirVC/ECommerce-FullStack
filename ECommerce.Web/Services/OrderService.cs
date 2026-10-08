using System.Text;
using System.Text.Json;
using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Base;
namespace ECommerce.Web.Services;
public class OrderService : ApiServiceBase, IOrderService
{
    public OrderService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, httpContextAccessor)
    {
    }
    public async Task<OrderResponseDto?> CheckoutAsync(CheckoutDto dto)
    {
        var client = CreateClient();
        var response = await client.PostAsync(
            "api/orders",
            new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                "application/json"));

        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeAsync<OrderResponseDto>(response);
    }

    public async Task<List<OrderResponseDto>> GetMyOrdersAsync()
    {
        var client = CreateClient();
        var response = await client.GetAsync("api/orders");
        if (!response.IsSuccessStatusCode)
            return new();
        return await DeserializeAsync<List<OrderResponseDto>>(response)
               ?? new();
    }
    public async Task<OrderResponseDto?> GetOrderByIdAsync(int id)
    {
        var client = CreateClient();
        var response = await client.GetAsync($"api/orders/{id}");
        if (!response.IsSuccessStatusCode)
            return null;
        return await DeserializeAsync<OrderResponseDto>(response);
    }
}
