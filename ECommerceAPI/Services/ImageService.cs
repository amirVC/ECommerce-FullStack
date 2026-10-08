using ECommerceAPI.Exceptions;
using ECommerceAPI.Options;
using ECommerceAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ECommerceAPI.Services;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ImageOptimizationOptions _options;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public ImageService(
        IWebHostEnvironment env,
        IHttpContextAccessor httpContextAccessor,
        IOptions<ImageOptimizationOptions> options)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
    }

    public Task<(string ImageUrl, string ThumbnailUrl)> SaveProductImageAsync(IFormFile file)
        => SaveOptimizedAsync(file, "products");

    public Task<(string ImageUrl, string ThumbnailUrl)> SaveReviewImageAsync(IFormFile file)
        => SaveOptimizedAsync(file, "reviews");

    private async Task<(string ImageUrl, string ThumbnailUrl)> SaveOptimizedAsync(IFormFile file, string subFolder)
    {
        ValidateFile(file);

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", subFolder);
        Directory.CreateDirectory(uploadsFolder);

        var fileId = Guid.NewGuid().ToString();
        var fullFileName = $"{fileId}.webp";
        var thumbFileName = $"{fileId}_thumb.webp";

        var fullPath = Path.Combine(uploadsFolder, fullFileName);
        var thumbPath = Path.Combine(uploadsFolder, thumbFileName);

        try
        {
            using var stream = file.OpenReadStream();
            using var image = await Image.LoadAsync(stream);

            // Strip EXIF/metadata — cuts size and avoids leaking camera/location data
            image.Metadata.ExifProfile = null;

            using (var full = image.Clone(ctx => ctx.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max, // never upscale, keeps aspect ratio
                Size = new Size(_options.MaxWidth, _options.MaxHeight)
            })))
            {
                await full.SaveAsWebpAsync(fullPath, new WebpEncoder
                {
                    Quality = _options.Quality,
                    FileFormat = WebpFileFormatType.Lossy
                });
            }

            using (var thumb = image.Clone(ctx => ctx.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Crop, // fixed-size square, good for grids/cards
                Size = new Size(_options.ThumbnailWidth, _options.ThumbnailHeight),
                Position = AnchorPositionMode.Center
            })))
            {
                await thumb.SaveAsWebpAsync(thumbPath, new WebpEncoder
                {
                    Quality = _options.ThumbnailQuality,
                    FileFormat = WebpFileFormatType.Lossy
                });
            }
        }
        catch (UnknownImageFormatException)
        {
            throw new BadRequestException("The uploaded file is not a valid image.");
        }

        var request = _httpContextAccessor.HttpContext!.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";

        var imageUrl = $"{baseUrl}/uploads/{subFolder}/{fullFileName}";
        var thumbnailUrl = $"{baseUrl}/uploads/{subFolder}/{thumbFileName}";
        return (imageUrl, thumbnailUrl);
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new BadRequestException("No file uploaded.");

        if (file.Length > MaxFileSizeBytes)
            throw new BadRequestException("Image must be smaller than 5MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new BadRequestException("Only .jpg, .jpeg, .png, .webp files are allowed.");
    }

    public void DeleteProductImage(string? imageUrl) => DeleteFile(imageUrl, "products");
    public void DeleteProductThumbnail(string? thumbnailUrl) => DeleteFile(thumbnailUrl, "products");
    public void DeleteReviewImage(string? imageUrl) => DeleteFile(imageUrl, "reviews");
    public void DeleteReviewThumbnail(string? thumbnailUrl) => DeleteFile(thumbnailUrl, "reviews");

    private void DeleteFile(string? url, string subFolder)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        if (!url.Contains($"/uploads/{subFolder}/")) return;

        var fileName = Path.GetFileName(new Uri(url).LocalPath);
        var filePath = Path.Combine(_env.WebRootPath, "uploads", subFolder, fileName);

        if (File.Exists(filePath))
            File.Delete(filePath);
    }
}