using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Extensions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services;

public class AdminProductService : IAdminProductService
{
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;

    public AdminProductService(AppDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<PagedResultDto<AdminProductResponseDto>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Name);

        var totalCount = await query.CountAsync();

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = products.Select(p => new AdminProductResponseDto
        {
            Id = p.Id,
            SKU = p.SKU,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            ImageUrl = p.ImageUrl,
            CategoryName = p.Category.Name,
            SalePrice = p.SalePrice,
            SaleStartDate = p.SaleStartDate,
            SaleEndDate = p.SaleEndDate,
            IsOnSale = p.IsOnSale(),
            EffectivePrice = p.EffectivePrice()
        }).ToList();

        return new PagedResultDto<AdminProductResponseDto>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
    public async Task<PagedResultDto<AdminProductResponseDto>> SearchAsync(ProductSearchDto dto)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(dto.Search))
        {
            var search = dto.Search.Trim();

            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{search}%") ||
                EF.Functions.ILike(p.SKU, $"%{search}%") ||
                EF.Functions.ILike(p.Category.Name, $"%{search}%"));
        }

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((dto.Page - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .ToListAsync();

        var items = products.Select(p => new AdminProductResponseDto
        {
            Id = p.Id,
            SKU = p.SKU,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            ImageUrl = p.ImageUrl,
            CategoryName = p.Category.Name,
            SalePrice = p.SalePrice,
            SaleStartDate = p.SaleStartDate,
            SaleEndDate = p.SaleEndDate,
            IsOnSale = p.IsOnSale(),
            EffectivePrice = p.EffectivePrice()
        }).ToList();

        return new PagedResultDto<AdminProductResponseDto>
        {
            Items = items,
            CurrentPage = dto.Page,
            PageSize = dto.PageSize,
            TotalCount = totalCount
        };
    }
    public async Task<AdminProductResponseDto> CreateAsync(AdminCreateProductDto dto)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId);

        if (category == null)
            throw new Exception("Category not found.");

        ValidateSalePricing(dto.Price, dto.SalePrice, dto.SaleStartDate, dto.SaleEndDate);

        var sku = NormalizeSku(dto.SKU);

        if (await _context.Products.AnyAsync(p => p.SKU == sku))
            throw new BadRequestException($"A product with SKU '{sku}' already exists.");

        string? imageUrl = dto.ImageUrl;
        string? thumbnailUrl = null;
        if (dto.ImageFile != null)
        {
            (imageUrl, thumbnailUrl) = await _imageService.SaveProductImageAsync(dto.ImageFile);
        }

        var product = new Product
        {
            SKU = sku,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            ImageUrl = imageUrl,
            ThumbnailUrl = thumbnailUrl,
            CategoryId = dto.CategoryId,
            SalePrice = dto.SalePrice,
            SaleStartDate = ToUtc(dto.SaleStartDate),
            SaleEndDate = ToUtc(dto.SaleEndDate)
        };

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            var altText = string.IsNullOrWhiteSpace(dto.ImageAltText)
                ? dto.Name
                : dto.ImageAltText;

            product.Images.Add(new ProductImage
            {
                ImageUrl = imageUrl,
                ThumbnailUrl = thumbnailUrl ?? string.Empty,
                AltText = altText,
                IsPrimary = true,
                DisplayOrder = 0
            });
        }
        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return new AdminProductResponseDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CategoryName = category.Name,
            SalePrice = product.SalePrice,
            SaleStartDate = product.SaleStartDate,
            SaleEndDate = product.SaleEndDate,
            IsOnSale = product.IsOnSale(),
            EffectivePrice = product.EffectivePrice()
        };
    }
    public async Task<AdminProductResponseDto> UpdateAsync(
        int id,
        AdminUpdateProductDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            throw new NotFoundException("Product not found.");

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId);

        if (category == null)
            throw new NotFoundException("Category not found.");

        ValidateSalePricing(dto.Price, dto.SalePrice, dto.SaleStartDate, dto.SaleEndDate);

        var sku = NormalizeSku(dto.SKU);

        if (await _context.Products.AnyAsync(p => p.SKU == sku && p.Id != id))
            throw new BadRequestException($"A product with SKU '{sku}' already exists.");

        var effectiveAltText = string.IsNullOrWhiteSpace(dto.ImageAltText)
            ? dto.Name
            : dto.ImageAltText;
        var currentPrimary = product.Images.FirstOrDefault(i => i.IsPrimary);

        if (dto.ImageFile != null)
        {
            var (newImageUrl, newThumbnailUrl) = await _imageService.SaveProductImageAsync(dto.ImageFile);
            _imageService.DeleteProductImage(product.ImageUrl);
            _imageService.DeleteProductThumbnail(product.ThumbnailUrl);
            product.ImageUrl = newImageUrl;
            product.ThumbnailUrl = newThumbnailUrl;

            if (currentPrimary != null)
            {
                currentPrimary.ImageUrl = newImageUrl;
                currentPrimary.ThumbnailUrl = newThumbnailUrl;
                currentPrimary.AltText = effectiveAltText;
            }
            else
            {
                product.Images.Add(new ProductImage
                {
                    ImageUrl = newImageUrl,
                    ThumbnailUrl = newThumbnailUrl,
                    AltText = effectiveAltText,
                    IsPrimary = true,
                    DisplayOrder = 0
                });
            }
        }
        else if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            if (!string.Equals(product.ImageUrl, dto.ImageUrl, StringComparison.Ordinal))
            {
                _imageService.DeleteProductImage(product.ImageUrl);
                _imageService.DeleteProductThumbnail(product.ThumbnailUrl);
            }
            product.ImageUrl = dto.ImageUrl;
            product.ThumbnailUrl = null; 

            if (currentPrimary != null)
            {
                currentPrimary.ImageUrl = dto.ImageUrl;
                currentPrimary.ThumbnailUrl = string.Empty;
                currentPrimary.AltText = effectiveAltText;
            }
            else
            {
                product.Images.Add(new ProductImage
                {
                    ImageUrl = dto.ImageUrl,
                    ThumbnailUrl = string.Empty,
                    AltText = effectiveAltText,
                    IsPrimary = true,
                    DisplayOrder = 0
                });
            }
        }
        else
        {
            if (currentPrimary != null && !string.IsNullOrWhiteSpace(dto.ImageAltText))
            {
                currentPrimary.AltText = dto.ImageAltText;
            }
        }

        product.SKU = sku;
        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.CategoryId = dto.CategoryId;
        product.SalePrice = dto.SalePrice;
        product.SaleStartDate = ToUtc(dto.SaleStartDate);
        product.SaleEndDate = ToUtc(dto.SaleEndDate);

        await _context.SaveChangesAsync();

        return new AdminProductResponseDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CategoryName = category.Name,
            SalePrice = product.SalePrice,
            SaleStartDate = product.SaleStartDate,
            SaleEndDate = product.SaleEndDate,
            IsOnSale = product.IsOnSale(),
            EffectivePrice = product.EffectivePrice()
        };
    }
    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            throw new NotFoundException("Product not found.");

        _imageService.DeleteProductImage(product.ImageUrl);
        _imageService.DeleteProductThumbnail(product.ThumbnailUrl);

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();
    }
    public async Task<AdminUpdateProductDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return null;

        var primary = product.Images.FirstOrDefault(i => i.IsPrimary);

        return new AdminUpdateProductDto
        {
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            ImageAltText = primary?.AltText,
            CategoryId = product.CategoryId,
            SalePrice = product.SalePrice,
            SaleStartDate = product.SaleStartDate,
            SaleEndDate = product.SaleEndDate
        };
    }

    private static string NormalizeSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new BadRequestException("SKU is required.");

        return sku.Trim().ToUpperInvariant();
    }

    private static void ValidateSalePricing(decimal price, decimal? salePrice, DateTime? start, DateTime? end)
    {
        if (salePrice.HasValue && salePrice.Value >= price)
            throw new BadRequestException("Sale price must be lower than the regular price.");

        if (start.HasValue && end.HasValue && end.Value <= start.Value)
            throw new BadRequestException("Sale end date must be after the sale start date.");
    }
    private static DateTime? ToUtc(DateTime? dt)
    {
        if (!dt.HasValue) return null;

        return dt.Value.Kind switch
        {
            DateTimeKind.Utc => dt.Value,
            DateTimeKind.Local => dt.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt.Value, DateTimeKind.Local).ToUniversalTime()
        };
    }
}