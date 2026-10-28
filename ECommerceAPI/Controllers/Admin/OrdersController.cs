
using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class OrdersController : ControllerBase
{
    private readonly IAdminOrderService _orderService;

    public OrdersController(IAdminOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var orders = await _orderService.GetAllAsync(page, pageSize);
        return Ok(orders);
    }

    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetById(Guid publicId)
    {
        var order = await _orderService.GetByIdAsync(publicId);

        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [HttpPut("{publicId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid publicId,
        [FromBody] AdminUpdateOrderStatusDto dto)
    {
        await _orderService.UpdateStatusAsync(publicId, dto);
        return NoContent();
    }
}
