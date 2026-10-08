using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace ECommerce.Web.Controllers;
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IAuthService _authService;
    private readonly IRefundRequestService _refundRequestService;
    private readonly IShipmentService _shipmentService;
    public OrdersController(
        IOrderService orderService,
        IAuthService authService,
        IRefundRequestService refundRequestService,
        IShipmentService shipmentService)
    {
        _orderService = orderService;
        _authService = authService;
        _refundRequestService = refundRequestService;
        _shipmentService = shipmentService;
    }
    public async Task<IActionResult> Index()
    {
        if (!_authService.IsLoggedIn())
            return RedirectToAction("Login", "Account");
        var orders = await _orderService.GetMyOrdersAsync();
        return View(orders);
    }
    public async Task<IActionResult> Details(int id)
    {
        if (!_authService.IsLoggedIn())
            return RedirectToAction("Login", "Account");
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();

        var myRequests = await _refundRequestService.GetMineAsync();
        ViewBag.RefundRequest = myRequests
            .Where(r => r.OrderId == id)
            .OrderByDescending(r => r.RequestedAt)
            .FirstOrDefault();

        ViewBag.Shipment = await _shipmentService.GetByOrderIdAsync(id);

        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> RequestRefund(int orderId, string reason)
    {
        if (!_authService.IsLoggedIn())
            return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["RefundRequestError"] = "Please tell us why you'd like a refund.";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        var result = await _refundRequestService.CreateAsync(orderId, reason);
        TempData[result != null ? "RefundRequestSuccess" : "RefundRequestError"] =
            result != null
                ? "Your refund request has been submitted and is awaiting review."
                : "Could not submit your refund request. It may already have a pending request, or the order isn't eligible.";

        return RedirectToAction(nameof(Details), new { id = orderId });
    }
}