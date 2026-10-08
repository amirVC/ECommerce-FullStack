using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class ShipmentsController : Controller
{
    private readonly IShipmentService _shipmentService;

    public ShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    // GET /Admin/Shipments/Manage/5  (5 = orderId)
    public async Task<IActionResult> Manage(int orderId)
    {
        ViewBag.OrderId = orderId;
        var shipment = await _shipmentService.GetByOrderIdAsync(orderId);
        return View(shipment); // null -> view renders the "create shipment" form
    }

    [HttpPost]
    public async Task<IActionResult> Create(int orderId, string? carrier, string? trackingNumber)
    {
        await _shipmentService.CreateForOrderAsync(orderId, new CreateShipmentDto
        {
            Carrier = carrier,
            TrackingNumber = trackingNumber
        });

        return RedirectToAction(nameof(Manage), new { orderId });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int orderId, string status, string? note, string? carrier, string? trackingNumber)
    {
        await _shipmentService.UpdateStatusAsync(orderId, new UpdateShipmentStatusDto
        {
            Status = status,
            Note = note,
            Carrier = carrier,
            TrackingNumber = trackingNumber
        });

        return RedirectToAction(nameof(Manage), new { orderId });
    }
}