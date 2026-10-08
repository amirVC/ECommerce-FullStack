using System;
using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class CampaignsController : Controller
{
    private readonly IAdminCampaignService _campaignService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public CampaignsController(
        IAdminCampaignService campaignService,
        IProductService productService,
        ICategoryService categoryService)
    {
        _campaignService = campaignService;
        _productService = productService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var campaigns = await _campaignService.GetAllAsync();

        return View(campaigns);
    }

    public async Task<IActionResult> Create()
    {
        var products = await _productService.GetProductsAsync();
        var categories = await _categoryService.GetAllAsync();

        ViewBag.Products = products.Items;
        ViewBag.Categories = categories;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AdminCreateCampaignDto dto)
    {
        if (!ModelState.IsValid)
        {
            var products = await _productService.GetProductsAsync();
            var categories = await _categoryService.GetAllAsync();

            ViewBag.Products = products.Items;
            ViewBag.Categories = categories;

            return View(dto);
        }

        try
        {
            await _campaignService.CreateAsync(dto);

            TempData["Success"] =
                "Campaign created successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;

            var products = await _productService.GetProductsAsync();
            var categories = await _categoryService.GetAllAsync();

            ViewBag.Products = products.Items;
            ViewBag.Categories = categories;

            return View(dto);
        }
    }
    public async Task<IActionResult> Edit(int id)
    {
        var campaign = await _campaignService.GetByIdAsync(id);

        if (campaign == null)
            return NotFound();

        var products = await _productService.GetProductsAsync();
        var categories = await _categoryService.GetAllAsync();

        ViewBag.Products = products.Items;
        ViewBag.Categories = categories;

        return View(campaign);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        AdminUpdateCampaignDto dto)
    {
        if (!ModelState.IsValid)
        {
            var products = await _productService.GetProductsAsync();
            var categories = await _categoryService.GetAllAsync();

            ViewBag.Products = products.Items;
            ViewBag.Categories = categories;

            var campaign = new CampaignDto
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive,
                ProductIds = dto.ProductIds ?? new List<int>(),
                CategoryIds = dto.CategoryIds ?? new List<int>()
            };

            return View(campaign);
        }

        try
        {
            var result = await _campaignService.UpdateAsync(id, dto);

            if (result == null)
                return NotFound();

            TempData["SuccessMessage"] =
                "Campaign updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;

            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _campaignService.DeleteAsync(id);

            TempData["SuccessMessage"] =
                "Campaign deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}