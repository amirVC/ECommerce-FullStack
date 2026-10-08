using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Extensions;
using ECommerceAPI.Models;
using ECommerceAPI.Models.QueryParameters;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IProductService _productService;
    private readonly IImageService _imageService;

    public ProductsController(
        AppDbContext context,
        IProductService productService,
        IImageService imageService)
    {
        _context = context;
        _productService = productService;
        _imageService = imageService;
    }

    [HttpGet]
    [ResponseCache(
        Duration = 60,
        Location = ResponseCacheLocation.Any,
        VaryByQueryKeys = new[] { "Search", "CategoryId", "SortBy", "Page", "PageSize" })]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryParameters query)
    {
        var products = await _productService.GetAllAsync(query);

        return Ok(products);
    }

    [HttpGet("{id}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product == null)
            return NotFound("Product not found.");

        return Ok(product);
    }

    [HttpGet("sku/{sku}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetBySku(string sku)
    {
        var product = await _productService.GetBySkuAsync(sku);

        if (product == null)
            return NotFound("Product not found.");

        return Ok(product);
    }

    [HttpGet("category/{categoryId}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetByCategory(int categoryId)
    {
        var products = await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Category)
            .Include(p => p.Images)
            .ToListAsync();

        var result = products.Select(p => p.ToResponseDto());

        return Ok(result);
    }
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(ProductDto dto)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == dto.CategoryId);

        if (!categoryExists)
            return BadRequest("Category not found.");

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        var result = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CategoryName = (await _context.Categories.FindAsync(dto.CategoryId))!.Name
        };

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, ProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound("Product not found.");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.ImageUrl = dto.ImageUrl;
        product.CategoryId = dto.CategoryId;

        await _context.SaveChangesAsync();

        var result = new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CategoryName = (await _context.Categories.FindAsync(dto.CategoryId))!.Name
        };

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound("Product not found.");

        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            _imageService.DeleteProductImage(product.ImageUrl);
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return Ok("Product deleted.");
    }
    [HttpPost("{id}/image")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadImage(int id, IFormFile image)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound("Product not found.");

        if (image == null)
            return BadRequest("No image uploaded.");

        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            _imageService.DeleteProductImage(product.ImageUrl);
            _imageService.DeleteProductThumbnail(product.ThumbnailUrl);
        }

        var (imageUrl, thumbnailUrl) = await _imageService.SaveProductImageAsync(image);
        product.ImageUrl = imageUrl;
        product.ThumbnailUrl = thumbnailUrl;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Image uploaded successfully.",
            ImageUrl = product.ImageUrl,
            ThumbnailUrl = product.ThumbnailUrl
        });
    }

    [HttpDelete("{id}/image")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteImage(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return NotFound("Product not found.");

        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            _imageService.DeleteProductImage(product.ImageUrl);
            _imageService.DeleteProductThumbnail(product.ThumbnailUrl);

            product.ImageUrl = null;
            product.ThumbnailUrl = null;

            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpGet("{id}/related")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetRelated(int id)
    {
        var related = await _productService.GetRelatedAsync(id);
        return Ok(related);
    }
}