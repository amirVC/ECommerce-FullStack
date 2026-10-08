using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/coupons")]
[Authorize(Roles = "Admin")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;
    private readonly IAuditLogService _auditLogService;
    public CouponsController(ICouponService couponService, IAuditLogService auditLogService)
    {
        _couponService = couponService;
        _auditLogService = auditLogService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _couponService.GetAllAsync(page, pageSize));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var coupon = await _couponService.GetByIdAsync(id);
        return coupon is null ? NotFound("Coupon not found.") : Ok(coupon);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminCreateCouponDto dto)
    {
        var created = await _couponService.CreateAsync(dto);

        await _auditLogService.LogAsync(
            action: "CouponCreated",
            entityName: "Coupon",
            entityId: created.Id.ToString(),
            details: $"Coupon \"{created.Code}\" created.");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminUpdateCouponDto dto)
    {
        var updated = await _couponService.UpdateAsync(id, dto);
        if (updated is null) return NotFound("Coupon not found.");

        await _auditLogService.LogAsync(
            action: "CouponUpdated",
            entityName: "Coupon",
            entityId: id.ToString(),
            details: $"Coupon \"{updated.Code}\" updated.");

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _couponService.DeleteAsync(id);
        if (!deleted) return NotFound("Coupon not found.");

        await _auditLogService.LogAsync(
            action: "CouponDeleted",
            entityName: "Coupon",
            entityId: id.ToString(),
            details: $"Coupon #{id} deleted.");

        return NoContent();
    }

}