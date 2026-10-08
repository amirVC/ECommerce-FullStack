using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace ECommerceAPI.Services;
public class ProductImageService : IProductImageService
{
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;
    public ProductImageService(
        AppDbContext context,
        IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }
    public async Task<List<ProductImageDto>> GetImagesAsync(int productId)
    {
        var exists = await _context.Products
            .AnyAsync(p => p.Id == productId);
        if (!exists)
            throw new NotFoundException("Product not found.");
        return await _context.ProductImages
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.DisplayOrder)
            .Select(i => new ProductImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                ThumbnailUrl = i.ThumbnailUrl,
                AltText = i.AltText,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder
            })
            .ToListAsync();
    }
    public async Task<ProductImageDto> AddImageAsync(
        int productId,
        AdminCreateProductImageDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null)
            throw new NotFoundException("Product not found.");
        var isFirstImage = !product.Images.Any();
        var isPrimary = isFirstImage;
        var displayOrder = product.Images.Any()
            ? product.Images.Max(i => i.DisplayOrder) + 1
            : 0;

        var (imageUrl, thumbnailUrl) = await _imageService.SaveProductImageAsync(dto.ImageFile);

        var image = new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            ThumbnailUrl = thumbnailUrl,
            AltText = dto.AltText,
            IsPrimary = isPrimary,
            DisplayOrder = displayOrder
        };
        _context.ProductImages.Add(image);

        if (isPrimary)
        {
            product.ImageUrl = image.ImageUrl;
        }

        await _context.SaveChangesAsync();
        return new ProductImageDto
        {
            Id = image.Id,
            ImageUrl = image.ImageUrl,
            ThumbnailUrl = image.ThumbnailUrl,
            AltText = image.AltText,
            IsPrimary = image.IsPrimary,
            DisplayOrder = image.DisplayOrder
        };
    }
    public async Task DeleteImageAsync(int imageId)
    {
        var image = await _context.ProductImages
            .FirstOrDefaultAsync(i => i.Id == imageId);
        if (image == null)
            throw new NotFoundException("Image not found.");
        bool wasPrimary = image.IsPrimary;
        int productId = image.ProductId;
        _imageService.DeleteProductImage(image.ImageUrl);
        _imageService.DeleteProductThumbnail(image.ThumbnailUrl);
        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync();
        if (wasPrimary)
        {
            var newPrimary = await _context.ProductImages
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.DisplayOrder)
                .FirstOrDefaultAsync();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (newPrimary != null)
            {
                newPrimary.IsPrimary = true;

                if (product != null)
                {
                    product.ImageUrl = newPrimary.ImageUrl;
                }
            }
            else if (product != null)
            {
                product.ImageUrl = null;
            }

            await _context.SaveChangesAsync();
        }
    }
    public async Task SetPrimaryImageAsync(int imageId)
    {
        var image = await _context.ProductImages
            .FirstOrDefaultAsync(i => i.Id == imageId);
        if (image == null)
            throw new NotFoundException("Image not found.");

        await _context.ProductImages
            .Where(i => i.ProductId == image.ProductId && i.Id != imageId)
            .ExecuteUpdateAsync(s => s.SetProperty(i => i.IsPrimary, false));

        image.IsPrimary = true;

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == image.ProductId);
        if (product != null)
        {
            product.ImageUrl = image.ImageUrl;
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAltTextAsync(int imageId, string altText)
    {
        var image = await _context.ProductImages.FindAsync(imageId);

        if (image != null)
        {
            image.AltText = altText;
            await _context.SaveChangesAsync();
        }
    }
}