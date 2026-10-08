using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class CouponsController : Controller
{
    private readonly IAdminCouponService _couponService;
    private readonly ICategoryService _categoryService;
    private readonly IAdminProductService _productService;

    public CouponsController(
        IAdminCouponService couponService,
        ICategoryService categoryService,
        IAdminProductService productService)
    {
        _couponService = couponService;
        _categoryService = categoryService;
        _productService = productService;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var result = await _couponService.GetAllAsync(page, 20);

        ViewBag.CurrentPage = result.CurrentPage;
        ViewBag.TotalPages = result.TotalPages;

        return View(result.Items);
    }

    public async Task<IActionResult> Create()
    {
        var model = new AdminCreateCouponViewModel
        {
            Categories = await _categoryService.GetAllAsync(),
            Products = await _productService.GetAllAsync()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AdminCreateCouponViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await _categoryService.GetAllAsync();
            model.Products = await _productService.GetAllAsync();
            return View(model);
        }

        try
        {
            await _couponService.CreateAsync(model.Coupon);
            TempData["SuccessMessage"] = "Coupon created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Categories = await _categoryService.GetAllAsync();
            model.Products = await _productService.GetAllAsync();
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var coupon = await _couponService.GetByIdAsync(id);
        if (coupon == null) return NotFound();

        var model = new AdminEditCouponViewModel
        {
            Id = coupon.Id,
            Code = coupon.Code,
            TimesUsed = coupon.TimesUsed,
            Coupon = new AdminUpdateCouponDto
            {
                Description = coupon.Description,
                DiscountType = coupon.DiscountType,
                DiscountValue = coupon.DiscountValue,
                MaxDiscountAmount = coupon.MaxDiscountAmount,
                Scope = coupon.Scope,
                MinimumOrderAmount = coupon.MinimumOrderAmount,
                UsageLimitTotal = coupon.UsageLimitTotal,
                UsageLimitPerUser = coupon.UsageLimitPerUser,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                IsActive = coupon.IsActive,
                ProductIds = coupon.ProductIds,
                CategoryIds = coupon.CategoryIds
            },
            Categories = await _categoryService.GetAllAsync(),
            Products = await _productService.GetAllAsync()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, AdminEditCouponViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Id = id;
            model.Categories = await _categoryService.GetAllAsync();
            model.Products = await _productService.GetAllAsync();
            return View(model);
        }

        try
        {
            await _couponService.UpdateAsync(id, model.Coupon);
            TempData["SuccessMessage"] = "Coupon updated successfully.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Id = id;
            model.Categories = await _categoryService.GetAllAsync();
            model.Products = await _productService.GetAllAsync();
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _couponService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}