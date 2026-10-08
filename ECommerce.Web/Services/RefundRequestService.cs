using System.Text;
using System.Text.Json;
using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Base;
namespace ECommerce.Web.Services;
public class RefundRequestService : ApiServiceBase, IRefundRequestService
{
    public RefundRequestService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, httpContextAccessor)
    {
    }

    public async Task<RefundRequestDto?> CreateAsync(int orderId, string reason)
    {
        var client = CreateClient();
        var response = await client.PostAsync(
            "api/refund-requests",
            new StringContent(
                JsonSerializer.Serialize(new CreateRefundRequestDto { OrderId = orderId, Reason = reason }),
                Encoding.UTF8,
                "application/json"));

        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeAsync<RefundRequestDto>(response);
    }

    public async Task<List<RefundRequestDto>> GetMineAsync()
    {
        var client = CreateClient();
        var response = await client.GetAsync("api/refund-requests/mine");
        if (!response.IsSuccessStatusCode)
            return new();
        return await DeserializeAsync<List<RefundRequestDto>>(response) ?? new();
    }

    public async Task<List<AdminRefundRequestDto>> GetAllAsync()
    {
        var client = CreateClient();
        var response = await client.GetAsync("api/admin/refund-requests");
        if (!response.IsSuccessStatusCode)
            return new();
        return await DeserializeAsync<List<AdminRefundRequestDto>>(response) ?? new();
    }

    public async Task<AdminRefundRequestDto?> ApproveAsync(int id, string? adminNote)
    {
        var client = CreateClient();
        var response = await client.PostAsync(
            $"api/admin/refund-requests/{id}/approve",
            new StringContent(
                JsonSerializer.Serialize(new ReviewRefundRequestDto { AdminNote = adminNote }),
                Encoding.UTF8,
                "application/json"));

        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeAsync<AdminRefundRequestDto>(response);
    }

    public async Task<AdminRefundRequestDto?> RejectAsync(int id, string? adminNote)
    {
        var client = CreateClient();
        var response = await client.PostAsync(
            $"api/admin/refund-requests/{id}/reject",
            new StringContent(
                JsonSerializer.Serialize(new ReviewRefundRequestDto { AdminNote = adminNote }),
                Encoding.UTF8,
                "application/json"));

        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeAsync<AdminRefundRequestDto>(response);
    }
}