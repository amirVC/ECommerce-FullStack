using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Http;

namespace ECommerce.Web.Services;

public class AdminAuditLogService : IAdminAuditLogService
{
    private readonly IApiClient _apiClient;

    public AdminAuditLogService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<AuditLogPagedResultDto> GetAllAsync(
        int page = 1,
        int pageSize = 20,
        string? logAction = null,
        string? entityName = null,
        int? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(logAction))
            query.Add($"action={Uri.EscapeDataString(logAction)}");

        if (!string.IsNullOrWhiteSpace(entityName))
            query.Add($"entityName={Uri.EscapeDataString(entityName)}");

        if (userId.HasValue)
            query.Add($"userId={userId.Value}");

        if (fromDate.HasValue)
            query.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");

        if (toDate.HasValue)
            query.Add($"toDate={toDate.Value:yyyy-MM-dd}");

        var url = $"api/admin/audit-logs?{string.Join("&", query)}";

        return await _apiClient.GetAsync<AuditLogPagedResultDto>(url)
               ?? new AuditLogPagedResultDto();
    }
}