using ECommerce.Web.DTOs;
using ECommerce.Web.Helpers;
using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class RecentlyViewedController : Controller
{
    private readonly IRecentlyViewedService _recentlyViewedService;
    private readonly IAuthService _authService;

    public RecentlyViewedController(
        IRecentlyViewedService recentlyViewedService,
        IAuthService authService)
    {
        _recentlyViewedService = recentlyViewedService;
        _authService = authService;
    }

    private bool IsLoggedIn =>
        !string.IsNullOrWhiteSpace(_authService.GetToken());

    public async Task<IActionResult> Index()
    {
        List<RecentlyViewedDto> items;

        if (IsLoggedIn)
        {
            try
            {
                items = await _recentlyViewedService.GetAsync();
            }
            catch
            {
                items = new List<RecentlyViewedDto>();
            }
        }
        else
        {
            items = GuestRecentlyViewedHelper.Get(HttpContext.Session);
        }

        return View(items);
    }

    [HttpPost]
    public async Task<IActionResult> Clear()
    {
        if (IsLoggedIn)
        {
            await _recentlyViewedService.ClearAsync();
        }
        else
        {
            GuestRecentlyViewedHelper.Clear(HttpContext.Session);
        }

        return RedirectToAction(nameof(Index));
    }
}