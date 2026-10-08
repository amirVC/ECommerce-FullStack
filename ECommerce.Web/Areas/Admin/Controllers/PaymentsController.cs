using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // GET /Admin/Payments
        [HttpGet]
        public async Task<IActionResult> Index(
            PaymentStatus? status,
            PaymentProvider? provider,
            int? orderId,
            int? userId,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1)
        {
            var query = new AdminPaymentQuery
            {
                Status = status,
                Provider = provider,
                OrderId = orderId,
                UserId = userId,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = 20
            };

            var result = await _paymentService.GetAllForAdminAsync(query);

            ViewBag.Status = status;
            ViewBag.Provider = provider;
            ViewBag.OrderId = orderId;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(result);
        }

        // POST /Admin/Payments/Refund -- full refund of the remaining balance on the order's payment.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(int orderId)
        {
            var result = await _paymentService.RefundAsync(orderId, amount: null, reason: "Refunded by admin");

            TempData[result != null ? "Success" : "Error"] = result != null
                ? $"Refund processed for order #{orderId}."
                : $"Could not process refund for order #{orderId}.";

            return RedirectToAction(nameof(Index));
        }
    }
}
