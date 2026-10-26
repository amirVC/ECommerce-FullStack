
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

    public async Task<IActionResult> Increase(Guid publicId)
    {
        try
        {
            await _cartService.IncreaseQuantityAsync(publicId);
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Decrease(Guid publicId)
    {
        try
        {
            await _cartService.DecreaseQuantityAsync(publicId);
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Remove(Guid publicId)
    {
        try
        {
            await _cartService.RemoveFromCartAsync(publicId);
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

    
[HttpPost("/Cart/Add/{slug}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Add(
    string slug,
    int quantity = 1)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return NotFound();

        if (quantity <= 0)
        {
            TempData["Error"] = "Quantity must be greater than zero.";
            return RedirectToAction(nameof(Index));
        }

        var product = await _productService.GetProductBySlugAsync(slug);

        if (product == null)
            return NotFound();

        try
        {
            await _cartService.AddToCartAsync(product, quantity);

            TempData["Success"] = $"{product.Name} added to cart.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

}
