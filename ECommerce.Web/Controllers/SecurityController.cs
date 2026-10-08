using ECommerce.Web.Interfaces;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

[Authorize]
public class SecurityController : Controller
{
    private readonly IAuthService _authService;

    public SecurityController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var enabled = await _authService.IsTotpEnabledAsync();

        return View(new SecurityIndexViewModel
        {
            TwoFactorEnabled = enabled
        });
    }

    [HttpGet]
    public async Task<IActionResult> Enable()
    {
        var setup = await _authService.SetupTotpAsync();

        if (setup == null)
        {
            TempData["ErrorMessage"] = "Could not start two-factor setup. Please try again.";
            return RedirectToAction(nameof(Index));
        }

        return View(new EnableTotpViewModel
        {
            SecretKey = setup.SecretKey,
            QrCodeUri = setup.QrCodeUri
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable(EnableTotpViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var success = await _authService.EnableTotpAsync(model.Code);

        if (!success)
        {
            ModelState.AddModelError(
                "",
                "That code is invalid or has expired. Please try again.");

            return View(model);
        }

        TempData["SuccessMessage"] = "Two-factor authentication has been enabled.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(DisableTotpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please enter the 6-digit code from your authenticator app.";
            return RedirectToAction(nameof(Index));
        }

        var success = await _authService.DisableTotpAsync(model.Code);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Two-factor authentication has been disabled."
            : "That code is invalid.";

        return RedirectToAction(nameof(Index));
    }
}