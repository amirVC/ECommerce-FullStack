using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class ReviewsController : Controller
{
    private readonly IReviewService _reviewService;
    private readonly IAuthService _authService;

    public ReviewsController(IReviewService reviewService, IAuthService authService)
    {
        _reviewService = reviewService;
        _authService = authService;
    }

    private bool IsLoggedIn => !string.IsNullOrWhiteSpace(_authService.GetToken());

    public async Task<IActionResult> MyReviews()
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyReviews") });

        var reviews = await _reviewService.GetMyReviewsAsync();
        return View(reviews);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account");

        await _reviewService.DeleteReviewAsync(id);
        TempData["Success"] = "Review deleted.";
        return RedirectToAction("MyReviews");
    }
}