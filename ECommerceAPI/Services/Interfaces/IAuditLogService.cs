using ECommerceAPI.DTOs;
using ECommerceAPI.QueryParameters;

namespace ECommerceAPI.Services.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        string action,
        string? entityName = null,
        string? entityId = null,
        string? details = null,
        int? userId = null,
        string? userEmail = null,
        string? ipAddress = null);   
    Task<PagedResultDto<AuditLogDto>> GetAllAsync(AuditLogQueryParameters query);
}