using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
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

    public async Task<IActionResult> Index()
    {
        var orders = await _orderService.GetAllAsync();

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetByIdAsync(id);

        if (order == null)
            return NotFound();

        ViewBag.Shipment = await _shipmentService.GetByOrderIdAsync(id);

        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(
    int id,
    AdminUpdateOrderStatusDto dto)
    {
        await _orderService.UpdateStatusAsync(id, dto);

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Refund(int id, string? reason)
    {
        var result = await _paymentService.RefundAsync(id, amount: null, reason: reason);

        TempData["RefundResult"] = result != null
            ? $"Refunded {result.RefundedAmount:C} — payment status is now {result.Status}."
            : "Refund failed. Check that this order has a successful payment to refund.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // Phase 18: shipping

    [HttpPost]
    public async Task<IActionResult> CreateShipment(int id, string? carrier, string? trackingNumber)
    {
        var result = await _shipmentService.CreateForOrderAsync(id, new CreateShipmentDto
        {
            Carrier = carrier,
            TrackingNumber = trackingNumber
        });

        TempData["ShipmentResult"] = result != null
            ? "Shipment created."
            : "Could not create shipment — it may already exist for this order.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateShipmentStatus(
        int id, string status, string? note, string? carrier, string? trackingNumber)
    {
        var result = await _shipmentService.UpdateStatusAsync(id, new UpdateShipmentStatusDto
        {
            Status = status,
            Note = note,
            Carrier = carrier,
            TrackingNumber = trackingNumber
        });

        TempData["ShipmentResult"] = result != null
            ? $"Shipment status updated to {result.Status}."
            : "Could not update shipment status.";

        return RedirectToAction(nameof(Details), new { id });
    }
}
