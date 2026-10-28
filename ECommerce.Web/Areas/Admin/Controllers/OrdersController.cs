
using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/Orders")]
public class OrdersController : Controller
{
    private readonly IAdminOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly IShipmentService _shipmentService;

    public OrdersController(
        IAdminOrderService orderService,
        IPaymentService paymentService,
        IShipmentService shipmentService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
        _shipmentService = shipmentService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var orders = await _orderService.GetAllAsync();
        return View(orders);
    }

    [HttpGet("Details/{publicId:guid}")]
    public async Task<IActionResult> Details(Guid publicId)
    {
        var order = await _orderService.GetByIdAsync(publicId);

        if (order == null)
            return NotFound();

        ViewBag.Shipment =
            await _shipmentService.GetByOrderIdAsync(order.Id);

        return View(order);
    }
    [HttpPost("UpdateStatus/{publicId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        Guid publicId,
        AdminUpdateOrderStatusDto dto)
    {
        var order = await _orderService.GetByIdAsync(publicId);

        if (order == null)
            return NotFound();

        await _orderService.UpdateStatusAsync(publicId, dto);

        return RedirectToAction(nameof(Details), new { publicId });
    }

    [HttpPost("Refund/{publicId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refund(
        Guid publicId,
        string? reason)
    {
        var order = await _orderService.GetByIdAsync(publicId);

        if (order == null)
            return NotFound();

        var result = await _paymentService.RefundAsync(
            order.Id,
            amount: null,
            reason: reason);

        TempData["RefundResult"] = result != null
            ? $"Refunded {result.RefundedAmount:C} — payment status is now {result.Status}."
            : "Refund failed. Check that this order has a successful payment to refund.";

        return RedirectToAction(nameof(Details), new { publicId });
    }

    [HttpPost("CreateShipment/{publicId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateShipment(
        Guid publicId,
        string? carrier,
        string? trackingNumber)
    {
        var order = await _orderService.GetByIdAsync(publicId);

        if (order == null)
            return NotFound();

        var result = await _shipmentService.CreateForOrderAsync(
            order.Id,
            new CreateShipmentDto
            {
                Carrier = carrier,
                TrackingNumber = trackingNumber
            });

        TempData["ShipmentResult"] = result != null
            ? "Shipment created."
            : "Could not create shipment — it may already exist for this order.";

        return RedirectToAction(nameof(Details), new { publicId });
    }

    [HttpPost("UpdateShipmentStatus/{publicId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateShipmentStatus(
        Guid publicId,
        string status,
        string? note,
        string? carrier,
        string? trackingNumber)
    {
        var order = await _orderService.GetByIdAsync(publicId);

        if (order == null)
            return NotFound();

        var result = await _shipmentService.UpdateStatusAsync(
            order.Id,
            new UpdateShipmentStatusDto
            {
                Status = status,
                Note = note,
                Carrier = carrier,
                TrackingNumber = trackingNumber
            });

        TempData["ShipmentResult"] = result != null
            ? $"Shipment status updated to {result.Status}."
            : "Could not update shipment status.";

        return RedirectToAction(nameof(Details), new { publicId });
    }
}
