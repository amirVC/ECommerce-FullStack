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
   
    [HttpGet("Orders/{publicId:guid}")]
    public async Task<IActionResult> DetailsByPublicId(Guid publicId)
    {
        if (!_authService.IsLoggedIn())
            return RedirectToAction("Login", "Account");

        var order = await _orderService.GetOrderByPublicIdAsync(publicId);
        if (order == null)
            return NotFound();

      
        var myRequests = await _refundRequestService.GetMineAsync();
        ViewBag.RefundRequest = myRequests
            .Where(r => r.OrderId == order.Id)
            .OrderByDescending(r => r.RequestedAt)
            .FirstOrDefault();

        ViewBag.Shipment = await _shipmentService.GetByOrderIdAsync(order.Id);

        return View("Details", order);
    }

   
    public async Task<IActionResult> Details(int id)
    {
        if (!_authService.IsLoggedIn())
            return RedirectToAction("Login", "Account");

        var order = await _orderService.GetOrderByIdAsync(id);  
        if (order == null)
            return NotFound();

        return RedirectToActionPermanent(nameof(DetailsByPublicId), new { publicId = order.PublicId });
    }

    [HttpPost]
    public async Task<IActionResult> RequestRefund(Guid publicId, string reason)
    {
        if (!_authService.IsLoggedIn())
            return RedirectToAction("Login", "Account");

        var order = await _orderService.GetOrderByPublicIdAsync(publicId);
        if (order == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["RefundRequestError"] = "Please tell us why you'd like a refund.";
            return RedirectToAction(nameof(DetailsByPublicId), new { publicId });
        }

        var result = await _refundRequestService.CreateAsync(order.Id, reason);
        TempData[result != null ? "RefundRequestSuccess" : "RefundRequestError"] =
            result != null
                ? "Your refund request has been submitted and is awaiting review."
                : "Could not submit your refund request. It may already have a pending request, or the order isn't eligible.";

        return RedirectToAction(nameof(DetailsByPublicId), new { publicId });
    }
}