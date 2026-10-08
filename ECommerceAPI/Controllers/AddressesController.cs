using System.Security.Claims;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/addresses")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _service;
    public AddressesController(IAddressService service) => _service = service;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetMine() => Ok(await _service.GetMyAddressesAsync(UserId));

    [HttpPost]
    public async Task<IActionResult> Create(CreateAddressDto dto) => Ok(await _service.CreateAsync(UserId, dto));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateAddressDto dto) => Ok(await _service.UpdateAsync(UserId, id, dto));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(UserId, id);
        return NoContent();
    }

    [HttpPost("{id}/default")]
    public async Task<IActionResult> SetDefault(int id)
    {
        await _service.SetDefaultAsync(UserId, id);
        return NoContent();
    }
}