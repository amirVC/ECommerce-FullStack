using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin/orders/{orderId}/shipment")]
[Authorize(Roles = "Admin")]
public class AdminShipmentsController : ControllerBase
{
    private readonly IShipmentService _service;
    public AdminShipmentsController(IShipmentService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create(int orderId, CreateShipmentDto dto) => Ok(await _service.CreateForOrderAsync(orderId, dto));

    [HttpPut("status")]
    public async Task<IActionResult> UpdateStatus(int orderId, UpdateShipmentStatusDto dto) => Ok(await _service.UpdateStatusAsync(orderId, dto));
}