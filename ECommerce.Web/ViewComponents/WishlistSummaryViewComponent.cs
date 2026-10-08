using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.ViewComponents;

public class WishlistSummaryViewComponent : ViewComponent
{
    private readonly IWishlistService _wishlistService;
    private readonly IAuthService _authService;

    public WishlistSummaryViewComponent(IWishlistService wishlistService, IAuthService authService)
    {
        _wishlistService = wishlistService;
        _authService = authService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var isLoggedIn = !string.IsNullOrWhiteSpace(_authService.GetToken());
        var count = isLoggedIn ? await _wishlistService.GetWishlistCountAsync() : 0;

        return View(count);
    }
}