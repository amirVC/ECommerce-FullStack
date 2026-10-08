using ECommerce.Web.Exceptions;
using ECommerce.Web.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IProductService _productService;

    public CartController(
        ICartService cartService,
        IProductService productService)
    {
        _cartService = cartService;
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var cart = await _cartService.GetCartAsync();

        return View(cart);
    }

    public async Task<IActionResult> Add(int id, int quantity = 1)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
            return NotFound();

        try
        {
            await _cartService.AddToCartAsync(product);
            TempData["Success"] = $"{product.Name} added to cart.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Increase(int id)
    {
        try
        {
            await _cartService.IncreaseQuantityAsync(id);
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Decrease(int id)
    {
        try
        {
            await _cartService.DecreaseQuantityAsync(id);
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Remove(int id)
    {
        try
        {
            await _cartService.RemoveFromCartAsync(id);
            TempData["Success"] = "Product removed from cart.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Clear()
    {
        try
        {
            await _cartService.ClearCartAsync();
            TempData["Success"] = "Cart cleared successfully.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(string code)
    {
        try
        {
            await _cartService.ApplyCouponAsync(code);
            TempData["Success"] = "Coupon applied!";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveCoupon()
    {
        try
        {
            await _cartService.RemoveCouponAsync();
            TempData["Success"] = "Coupon removed.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}