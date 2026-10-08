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

        return View(categories);
    }

    public IActionResult Create()
    {
        return View(new AdminCreateCategoryViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        AdminCreateCategoryViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _categoryService.CreateAsync(model.Category);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        var model = new AdminEditCategoryViewModel
        {
            Id = id,
            Category = new AdminUpdateCategoryDto
            {
                Name = category.Name
            }
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(
        int id,
        AdminEditCategoryViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _categoryService.UpdateAsync(
            id,
            model.Category);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }
}