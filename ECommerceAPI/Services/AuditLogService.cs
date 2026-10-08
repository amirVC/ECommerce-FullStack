using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.QueryParameters;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceAPI.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(
        string action,
        string? entityName = null,
        string? entityId = null,
        string? details = null,
        int? userId = null,
        string? userEmail = null,
        string? ipAddress = null)   // add this
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var resolvedUserId = userId ?? GetCurrentUserId(httpContext);
            var resolvedUserEmail = userEmail ?? GetCurrentUserEmail(httpContext);
            var resolvedIpAddress = ipAddress ?? httpContext?.Connection.RemoteIpAddress?.ToString();

            var entry = new AuditLog
            {
                UserId = resolvedUserId,
                UserEmail = resolvedUserEmail,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details,
                IpAddress = resolvedIpAddress,   
                CreatedAt = DateTime.UtcNow
            };

            _context.AuditLogs.Add(entry);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to write audit log entry. Action: {Action}, EntityName: {EntityName}, EntityId: {EntityId}",
                action,
                entityName,
                entityId);
        }
    }
    public async Task<PagedResultDto<AuditLogDto>> GetAllAsync(AuditLogQueryParameters query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize;

        var logsQuery = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Action))
            logsQuery = logsQuery.Where(l => l.Action == query.Action);

        if (!string.IsNullOrWhiteSpace(query.EntityName))
            logsQuery = logsQuery.Where(l => l.EntityName == query.EntityName);

        if (query.UserId.HasValue)
            logsQuery = logsQuery.Where(l => l.UserId == query.UserId.Value);

        if (query.FromDate.HasValue)
        {
            var fromDateUtc = DateTime.SpecifyKind(query.FromDate.Value.Date, DateTimeKind.Utc);
            logsQuery = logsQuery.Where(l => l.CreatedAt >= fromDateUtc);
        }

        if (query.ToDate.HasValue)
        {
            var toDateUtc = DateTime.SpecifyKind(query.ToDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            logsQuery = logsQuery.Where(l => l.CreatedAt < toDateUtc);
        }
        logsQuery = logsQuery.OrderByDescending(l => l.CreatedAt);

        var totalCount = await logsQuery.CountAsync();

        var items = await logsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AuditLogDto
            {
                Id = l.Id,
                UserId = l.UserId,
                UserEmail = l.UserEmail,
                Action = l.Action,
                EntityName = l.EntityName,
                EntityId = l.EntityId,
                Details = l.Details,
                IpAddress = l.IpAddress,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new PagedResultDto<AuditLogDto>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private static int? GetCurrentUserId(HttpContext? httpContext)
    {
        var idClaim = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : null;
    }

    private static string? GetCurrentUserEmail(HttpContext? httpContext) =>
        httpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
}