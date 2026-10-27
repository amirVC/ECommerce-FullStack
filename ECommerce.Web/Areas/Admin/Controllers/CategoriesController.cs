using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class CategoriesController : Controller
{
    private readonly IAdminCategoryService _categoryService;

    public CategoriesController(
        IAdminCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();

        categories = categories
            .OrderByDescending(c => c.Id)
            .ToList();

        return View(categories);
    }

    public IActionResult Create()
    {
        return View(new AdminCreateCategoryViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AdminCreateCategoryViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _categoryService.CreateAsync(model.Category);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{slug}")]
    public async Task<IActionResult> Edit(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return BadRequest();

        var category = await _categoryService.GetBySlugAsync(slug);

        if (category == null)
            return NotFound();

        var model = new AdminEditCategoryViewModel
        {
            Id = category.Id,
            Category = new AdminUpdateCategoryDto
            {
                Name = category.Name,
                Slug = category.Slug
            }
        };

        ViewBag.Slug = category.Slug;
        return View(model);
    }

    [HttpPost("Edit/{slug}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        string slug,
        AdminEditCategoryViewModel model)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Slug = slug;
            return View(model);
        }

        await _categoryService.UpdateAsync(slug, model.Category);

        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }
}