using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly ICartService _cartService;

    public AccountController(
        IAuthService authService,
        ICartService cartService)
    {
        _authService = authService;
        _cartService = cartService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (_authService.IsLoggedIn())
        {
            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        ViewBag.ReturnUrl = returnUrl;

        return View(new LoginDto());
    }

    [HttpPost]
    public async Task<IActionResult> Login(
        LoginDto model,
        string? returnUrl)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.LoginAsync(model);

        switch (result.Status)
        {
            case LoginStatus.Success:
                await _cartService.MergeGuestCartIntoUserCartAsync();

                if (!string.IsNullOrEmpty(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectToAction("Index", "Home");

            case LoginStatus.RequiresEmailOtp:
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.MaskedEmail = result.MaskedEmail;

                return View("VerifyEmailOtp", new VerifyCodeViewModel
                {
                    ChallengeToken = result.ChallengeToken!
                });

            case LoginStatus.RequiresTotp:
                ViewBag.ReturnUrl = returnUrl;

                return View("VerifyTotp", new VerifyCodeViewModel
                {
                    ChallengeToken = result.ChallengeToken!
                });

            case LoginStatus.AccountLocked:
                ModelState.AddModelError(
                    "",
                    result.LockoutMessage ??
                    "Too many failed attempts. Your account is temporarily locked.");

                return View(model);

            default:
                ModelState.AddModelError(
                    "",
                    "Invalid email, password, or your email has not been confirmed.");

                return View(model);
        }
    }

    // Step 1 of login for every user: confirm the emailed code.
    [HttpGet]
    public IActionResult VerifyEmailOtp()
    {
        // Only reachable as a result of the password step, which renders
        // this view directly rather than redirecting here. A bare GET means
        // the challenge token is missing.
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> VerifyEmailOtp(
        VerifyCodeViewModel model,
        string? returnUrl)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.VerifyEmailOtpAsync(
            model.ChallengeToken,
            model.Code);

        if (result.Status == LoginStatus.RequiresTotp)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View("VerifyTotp", new VerifyCodeViewModel
            {
                ChallengeToken = result.ChallengeToken!
            });
        }

        if (result.Status != LoginStatus.Success)
        {
            ModelState.AddModelError(
                "",
                "That code is invalid or has expired. Please try again.");

            return View(model);
        }

        await _cartService.MergeGuestCartIntoUserCartAsync();

        if (!string.IsNullOrEmpty(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    // Step 2, only for accounts with TOTP enabled from the Security page.
    [HttpGet]
    public IActionResult VerifyTotp()
    {
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> VerifyTotp(
        VerifyCodeViewModel model,
        string? returnUrl)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.VerifyTotpAsync(
            model.ChallengeToken,
            model.Code);

        if (result.Status != LoginStatus.Success)
        {
            ModelState.AddModelError(
                "",
                "That code is invalid or has expired. Please try again.");

            return View(model);
        }

        await _cartService.MergeGuestCartIntoUserCartAsync();

        if (!string.IsNullOrEmpty(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (_authService.IsLoggedIn())
            return RedirectToAction("Index", "Home");

        return View(new RegisterDto());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var message = await _authService.RegisterAsync(model);

        if (message == null)
        {
            ModelState.AddModelError(
                "",
                "Registration failed. The email may already be registered.");

            return View(model);
        }

        TempData["SuccessMessage"] = message;

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            ViewBag.Success = false;
            ViewBag.Message = "Invalid confirmation link.";

            return View();
        }

        var success =
            await _authService.ConfirmEmailAsync(token);

        ViewBag.Success = success;

        ViewBag.Message = success
            ? "Your email has been confirmed successfully. You can now log in."
            : "This confirmation link is invalid or has expired.";

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        if (_authService.IsLoggedIn())
            return RedirectToAction("Index", "Home");

        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(
    ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _authService.ForgotPasswordAsync(model.Email);
        }
        catch
        {
            // Don't reveal whether the email exists
            // or expose SMTP/API errors to the customer.
        }

        return RedirectToAction(
            nameof(ForgotPasswordConfirmation));
    }


    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["ErrorMessage"] =
                "Invalid password reset link.";

            return RedirectToAction(nameof(Login));
        }

        return View(new ResetPasswordViewModel
        {
            Token = token
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(
    ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var success =
            await _authService.ResetPasswordAsync(
                model.Token,
                model.NewPassword,
                model.ConfirmPassword);

        if (!success)
        {
            ModelState.AddModelError(
                "",
                "This password reset link is invalid or has expired.");

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Your password has been reset successfully. You can now log in.";

        return RedirectToAction(nameof(Login));
    }

}