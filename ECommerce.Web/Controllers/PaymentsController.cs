using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Controllers;

public class PaymentsController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IAuthService _authService;

    public PaymentsController(IPaymentService paymentService, IAuthService authService)
    {
        _paymentService = paymentService;
        _authService = authService;
    }

    // GET /Payments  -- the logged-in customer's own payment history.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!_authService.IsLoggedIn())
        {
            return RedirectToAction(
                "Login",
                "Account",
                new { returnUrl = Url.Action("Index", "Payments") });
        }

        var payments = await _paymentService.GetHistoryAsync();
        return View(payments);
    }
}
