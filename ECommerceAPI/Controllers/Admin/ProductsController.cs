using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class ProductsController : ControllerBase
{
    private readonly IAdminProductService _adminProductService;

    public ProductsController(IAdminProductService adminProductService)
    {
        _adminProductService = adminProductService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var products = await _adminProductService.GetAllAsync(page, pageSize);

        return Ok(products);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] AdminCreateProductDto dto)
    {
        var product = await _adminProductService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product);
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        int id,
        [FromForm] AdminUpdateProductDto dto)
    {
        var product = await _adminProductService.UpdateAsync(id, dto);

        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _adminProductService.DeleteAsync(id);

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] ProductSearchDto dto)
    {
        var products = await _adminProductService.SearchAsync(dto);

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _adminProductService.GetByIdAsync(id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }
}