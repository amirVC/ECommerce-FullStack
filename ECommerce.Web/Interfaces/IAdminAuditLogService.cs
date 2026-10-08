using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IAdminAuditLogService
{
    Task<AuditLogPagedResultDto> GetAllAsync(
        int page = 1,
        int pageSize = 20,
        string? action = null,
        string? entityName = null,
        int? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
}