using ECommerce.Web.Exceptions;
using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class WishlistController : Controller
{
    private readonly IWishlistService _wishlistService;
    private readonly IProductService _productService;
    private readonly IAuthService _authService;

    public WishlistController(
        IWishlistService wishlistService,
        IProductService productService,
        IAuthService authService)
    {
        _wishlistService = wishlistService;
        _productService = productService;
        _authService = authService;
    }

    private bool IsLoggedIn => !string.IsNullOrWhiteSpace(_authService.GetToken());

    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index), "Wishlist") });

        var wishlist = await _wishlistService.GetWishlistAsync();
        return View(wishlist);
    }

    public async Task<IActionResult> Add(int id, string? returnUrl = null)
    {
        if (!IsLoggedIn)
            return RedirectToAction("Login", "Account", new { returnUrl });

        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound();

        try
        {
            await _wishlistService.AddToWishlistAsync(id);
            TempData["Success"] = $"{product.Name} added to your wishlist.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectBack(returnUrl);
    }

    public async Task<IActionResult> Remove(int id, string? returnUrl = null)
    {
        try
        {
            await _wishlistService.RemoveFromWishlistAsync(id);
            TempData["Success"] = "Product removed from wishlist.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectBack(returnUrl);
    }

    public async Task<IActionResult> RemoveByProduct(int productId, string? returnUrl = null)
    {
        try
        {
            await _wishlistService.RemoveByProductAsync(productId);
            TempData["Success"] = "Product removed from wishlist.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectBack(returnUrl);
    }

    public async Task<IActionResult> Clear()
    {
        try
        {
            await _wishlistService.ClearWishlistAsync();
            TempData["Success"] = "Wishlist cleared successfully.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // Move item straight to cart
    public async Task<IActionResult> MoveToCart(int wishlistItemId, int productId, [FromServices] ICartService cartService)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return NotFound();

        try
        {
            await cartService.AddToCartAsync(product);
            await _wishlistService.RemoveFromWishlistAsync(wishlistItemId);
            TempData["Success"] = $"{product.Name} moved to your cart.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectBack(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAjax(int productId)
    {
        if (!IsLoggedIn)
            return Unauthorized();

        try
        {
            var wishlistedIds = await _wishlistService.GetWishlistedProductIdsAsync();
            var isCurrentlyWishlisted = wishlistedIds.Contains(productId);

            if (isCurrentlyWishlisted)
                await _wishlistService.RemoveByProductAsync(productId);
            else
                await _wishlistService.AddToWishlistAsync(productId);

            return Json(new { success = true, isWishlisted = !isCurrentlyWishlisted });
        }
        catch (ApiException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}