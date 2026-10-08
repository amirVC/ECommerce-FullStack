using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class AuditLogsController : Controller
{
    private readonly IAdminAuditLogService _auditLogService;

    public AuditLogsController(IAdminAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Index(
        int page = 1,
        string? logAction = null,
        string? entityName = null,
        int? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var result = await _auditLogService.GetAllAsync(
            page, 20, logAction, entityName, userId, fromDate, toDate);

        ViewBag.CurrentPage = result.CurrentPage;
        ViewBag.TotalPages = result.TotalPages;

        // Carried through to pagination links and re-populated filter inputs
        ViewBag.logAction = logAction;
        ViewBag.EntityName = entityName;
        ViewBag.UserId = userId;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        return View(result.Items);
    }
}