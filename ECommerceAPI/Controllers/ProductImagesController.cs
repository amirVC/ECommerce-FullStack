using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers;

[ApiController]
[Route("api")]
public class ProductImagesController : ControllerBase
{
    private readonly IProductImageService _productImageService;

    public ProductImagesController(IProductImageService productImageService)
    {
        _productImageService = productImageService;
    }

    // GET: api/products/{productId}/images
    [HttpGet("products/{productId}/images")]
    public async Task<IActionResult> GetImages(int productId)
    {
        var images = await _productImageService.GetImagesAsync(productId);

        return Ok(images);
    }

    // POST: api/products/{productId}/images
    [Authorize(Roles = "Admin")]
    [Consumes("multipart/form-data")]
    [HttpPost("products/{productId}/images")]
    public async Task<IActionResult> AddImage(
        int productId,
        [FromForm] AdminCreateProductImageDto dto)
    {
        var image = await _productImageService.AddImageAsync(productId, dto);

        return CreatedAtAction(
            nameof(GetImages),
            new { productId },
            image);
    }

    // PUT: api/product-images/{imageId}/primary
    [Authorize(Roles = "Admin")]
    [HttpPut("product-images/{imageId}/primary")]
    public async Task<IActionResult> SetPrimary(int imageId)
    {
        await _productImageService.SetPrimaryImageAsync(imageId);

        return NoContent();
    }

    // DELETE: api/product-images/{imageId}
    [Authorize(Roles = "Admin")]
    [HttpDelete("product-images/{imageId}")]
    public async Task<IActionResult> Delete(int imageId)
    {
        await _productImageService.DeleteImageAsync(imageId);

        return NoContent();
    }

    // PUT: api/product-images/{imageId}/alt-text
    [Authorize(Roles = "Admin")]
    [HttpPut("product-images/{imageId}/alt-text")]
    public async Task<IActionResult> UpdateAltText(int imageId, [FromBody] UpdateAltTextDto dto)
    {
        await _productImageService.UpdateAltTextAsync(imageId, dto.AltText);
        return NoContent();
    }

    // Add this class at the bottom of the file (or in a DTOs folder)
    public class UpdateAltTextDto
    {
        public string AltText { get; set; } = string.Empty;
    }
}