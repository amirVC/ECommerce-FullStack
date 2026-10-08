using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class RefundRequestsController : Controller
{
    private readonly IRefundRequestService _refundRequestService;

    public RefundRequestsController(IRefundRequestService refundRequestService)
    {
        _refundRequestService = refundRequestService;
    }

    public async Task<IActionResult> Index()
    {
        var requests = await _refundRequestService.GetAllAsync();
        return View(requests);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id, string? adminNote)
    {
        var result = await _refundRequestService.ApproveAsync(id, adminNote);
        TempData["RefundReviewResult"] = result != null
            ? $"Request #{id} approved — refund issued."
            : $"Could not approve request #{id}. It may no longer be pending.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id, string? adminNote)
    {
        var result = await _refundRequestService.RejectAsync(id, adminNote);
        TempData["RefundReviewResult"] = result != null
            ? $"Request #{id} rejected."
            : $"Could not reject request #{id}. It may no longer be pending.";
        return RedirectToAction(nameof(Index));
    }
}