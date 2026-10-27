
using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class CategoriesController : ControllerBase
{
    private readonly IAdminCategoryService _categoryService;

    public CategoriesController(IAdminCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug([FromRoute] string slug)
    {
        var category = await _categoryService.GetBySlugAsync(slug);

        if (category == null)
            return NotFound();

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] AdminCreateCategoryDto dto)
    {
        var category = await _categoryService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetBySlug),
            new { slug = category.Slug },
            category);
    }

    [HttpPut("{slug}")]
    public async Task<IActionResult> Update(
        [FromRoute] string slug,
        [FromBody] AdminUpdateCategoryDto dto)
    {
        var category = await _categoryService.UpdateAsync(slug, dto);

        if (category == null)
            return NotFound();

        return Ok(category);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}
