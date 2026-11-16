using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin/delivery-methods")]
[Authorize(Roles = "Admin")]
public class AdminDeliveryMethodsController : ControllerBase
{
    private readonly IDeliveryMethodService _service;

    public AdminDeliveryMethodsController(IDeliveryMethodService service)
    {
        _service = service;
    }

    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetById(Guid publicId)
    {
        var method = await _service.GetByPublicIdAsync(publicId);

        return method == null ? NotFound() : Ok(method);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDeliveryMethodDto dto)
    {
        return Ok(await _service.CreateAsync(dto));
    }

    [HttpPut("{publicId:guid}")]
    public async Task<IActionResult> Update(
        Guid publicId,
        UpdateDeliveryMethodDto dto)
    {
        return Ok(await _service.UpdateAsync(publicId, dto));
    }

    [HttpDelete("{publicId:guid}")]
    public async Task<IActionResult> Delete(Guid publicId)
    {
        await _service.DeleteAsync(publicId);

        return NoContent();
    }
}