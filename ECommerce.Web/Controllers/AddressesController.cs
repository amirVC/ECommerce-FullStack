using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class AddressesController : Controller
{
    private readonly IAddressService _addressService;
    private readonly IAuthService _authService;

    public AddressesController(IAddressService addressService, IAuthService authService)
    {
        _addressService = addressService;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        if (!_authService.IsLoggedIn())
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Index)) });

        var addresses = await _addressService.GetMyAddressesAsync();
        return View(addresses);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!_authService.IsLoggedIn())
            return RedirectToAction("Login", "Account");

        return View(new AddressFormViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddressFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = new CreateAddressDto
        {
            FullName = model.FullName,
            Phone = model.Phone,
            Line1 = model.Line1,
            Line2 = model.Line2,
            City = model.City,
            State = model.State,
            PostalCode = model.PostalCode,
            Country = model.Country,
            IsDefault = model.IsDefault
        };

        var created = await _addressService.CreateAsync(dto);
        if (created == null)
        {
            ModelState.AddModelError("", "Could not save the address. Please try again.");
            return View(model);
        }

        TempData["Success"] = "Address saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var addresses = await _addressService.GetMyAddressesAsync();
        var address = addresses.FirstOrDefault(a => a.Id == id);
        if (address == null)
            return NotFound();

        var model = new AddressFormViewModel
        {
            Id = address.Id,
            FullName = address.FullName,
            Phone = address.Phone,
            Line1 = address.Line1,
            Line2 = address.Line2,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country,
            IsDefault = address.IsDefault
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, AddressFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = new UpdateAddressDto
        {
            FullName = model.FullName,
            Phone = model.Phone,
            Line1 = model.Line1,
            Line2 = model.Line2,
            City = model.City,
            State = model.State,
            PostalCode = model.PostalCode,
            Country = model.Country,
            IsDefault = model.IsDefault
        };

        var updated = await _addressService.UpdateAsync(id, dto);
        if (updated == null)
        {
            ModelState.AddModelError("", "Could not update the address. Please try again.");
            return View(model);
        }

        TempData["Success"] = "Address updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _addressService.DeleteAsync(id);
        TempData["Success"] = "Address removed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> SetDefault(int id)
    {
        await _addressService.SetDefaultAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
