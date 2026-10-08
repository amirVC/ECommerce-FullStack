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
    public AdminDeliveryMethodsController(IDeliveryMethodService service) => _service = service;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var all = await _service.GetAllAsync();
        var method = all.FirstOrDefault(m => m.Id == id);
        return method == null ? NotFound() : Ok(method);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreateDeliveryMethodDto dto) => Ok(await _service.CreateAsync(dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateDeliveryMethodDto dto) => Ok(await _service.UpdateAsync(id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}