using System.Text;
using System.Text.Json;
using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Base;
namespace ECommerce.Web.Services;
public class PaymentService : ApiServiceBase, IPaymentService
{
    public PaymentService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, httpContextAccessor)
    {
    }

    public async Task<PaymentIntentResponseDto?> CreatePaymentIntentAsync(int orderId)
    {
        var client = CreateClient();
        var response = await client.PostAsync(
            "api/payments/create-intent",
            new StringContent(
                JsonSerializer.Serialize(new CreatePaymentIntentDto { OrderId = orderId }),
                Encoding.UTF8,
                "application/json"));

        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeAsync<PaymentIntentResponseDto>(response);
    }

    // ADDED: calls the API's admin-only refund endpoint. CreateClient() attaches the
    // logged-in admin's auth (same pattern as every other admin proxy service), so the
    // API's [Authorize(Roles = "Admin")] on that endpoint is what actually enforces this.
    public async Task<RefundResponseDto?> RefundAsync(int orderId, decimal? amount, string? reason)
    {
        var client = CreateClient();
        var response = await client.PostAsync(
            $"api/payments/{orderId}/refund",
            new StringContent(
                JsonSerializer.Serialize(new DirectRefundDto { Amount = amount, Reason = reason }),
                Encoding.UTF8,
                "application/json"));

        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeAsync<RefundResponseDto>(response);
    }

    // ADDED: the logged-in customer's own payment history.
    public async Task<List<PaymentDto>> GetHistoryAsync()
    {
        var client = CreateClient();
        var response = await client.GetAsync("api/payments/history");

        if (!response.IsSuccessStatusCode)
            return new List<PaymentDto>();

        return await DeserializeAsync<List<PaymentDto>>(response) ?? new List<PaymentDto>();
    }

    // ADDED: all payments across all users, for the admin Payments page. Same auth
    // pattern as RefundAsync above -- the API's [Authorize(Roles = "Admin")] enforces it.
    public async Task<AdminPaymentHistoryResultDto> GetAllForAdminAsync(AdminPaymentQuery query)
    {
        var client = CreateClient();
        var response = await client.GetAsync($"api/admin/payments{BuildQueryString(query)}");

        if (!response.IsSuccessStatusCode)
            return new AdminPaymentHistoryResultDto();

        return await DeserializeAsync<AdminPaymentHistoryResultDto>(response) ?? new AdminPaymentHistoryResultDto();
    }

    private static string BuildQueryString(AdminPaymentQuery query)
    {
        var parts = new List<string>
        {
            $"page={query.Page}",
            $"pageSize={query.PageSize}"
        };

        if (query.Status.HasValue) parts.Add($"status={(int)query.Status.Value}");
        if (query.Provider.HasValue) parts.Add($"provider={(int)query.Provider.Value}");
        if (query.OrderId.HasValue) parts.Add($"orderId={query.OrderId.Value}");
        if (query.UserId.HasValue) parts.Add($"userId={query.UserId.Value}");
        if (query.FromDate.HasValue) parts.Add($"fromDate={query.FromDate.Value:yyyy-MM-dd}");
        if (query.ToDate.HasValue) parts.Add($"toDate={query.ToDate.Value:yyyy-MM-dd}");

        return "?" + string.Join("&", parts);
    }
}