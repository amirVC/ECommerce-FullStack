using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class ReviewsController : Controller
{
    private readonly IAdminReviewService _adminReviewService;
    public ReviewsController(IAdminReviewService adminReviewService) => _adminReviewService = adminReviewService;

    public async Task<IActionResult> Index(int page = 1, string? status = null)
    {
        var result = await _adminReviewService.GetAllReviewsAsync(page, 20, status);
        ViewBag.StatusFilter = status;
        return View(result);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, string status, int page = 1, string? statusFilter = null)
    {
        await _adminReviewService.UpdateStatusAsync(id, status);
        return RedirectToAction("Index", new { page, status = statusFilter });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, int page = 1, string? statusFilter = null)
    {
        await _adminReviewService.DeleteReviewAsync(id);
        return RedirectToAction("Index", new { page, status = statusFilter });
    }
}