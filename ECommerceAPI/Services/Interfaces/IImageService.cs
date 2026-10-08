namespace ECommerceAPI.Services.Interfaces;

public interface IImageService
{
    Task<(string ImageUrl, string ThumbnailUrl)> SaveProductImageAsync(IFormFile file);
    void DeleteProductImage(string? imageUrl);
    void DeleteProductThumbnail(string? thumbnailUrl);

    Task<(string ImageUrl, string ThumbnailUrl)> SaveReviewImageAsync(IFormFile file);
    void DeleteReviewImage(string? imageUrl);
    void DeleteReviewThumbnail(string? thumbnailUrl);
}