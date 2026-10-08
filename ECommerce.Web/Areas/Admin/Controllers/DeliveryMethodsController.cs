using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class DeliveryMethodsController : Controller
{
    private readonly IDeliveryMethodService _service;

    public DeliveryMethodsController(IDeliveryMethodService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var methods = await _service.GetAllAsync();
        return View(methods);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateDeliveryMethodDto { IsActive = true });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDeliveryMethodDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var created = await _service.CreateAsync(dto);
        if (created == null)
        {
            ModelState.AddModelError("", "Could not create the delivery method.");
            return View(dto);
        }

        TempData["Success"] = "Delivery method created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var method = await _service.GetByIdAsync(id);
        if (method == null)
            return NotFound();

        var dto = new UpdateDeliveryMethodDto
        {
            Name = method.Name,
            Description = method.Description,
            Price = method.Price,
            EstimatedDaysMin = method.EstimatedDaysMin,
            EstimatedDaysMax = method.EstimatedDaysMax,
            IsActive = method.IsActive,
            SortOrder = method.SortOrder
        };

        ViewBag.Id = id;
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, UpdateDeliveryMethodDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Id = id;
            return View(dto);
        }

        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null)
        {
            ModelState.AddModelError("", "Could not update the delivery method.");
            ViewBag.Id = id;
            return View(dto);
        }

        TempData["Success"] = "Delivery method updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Delivery method deleted.";
        return RedirectToAction(nameof(Index));
    }
}
