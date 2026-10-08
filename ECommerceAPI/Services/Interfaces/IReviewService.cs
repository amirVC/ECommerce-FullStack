using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IReviewService
{
    Task<PagedResultDto<ReviewDto>> GetProductReviewsAsync(int productId, int page, int pageSize);
    Task<ReviewSummaryDto> GetProductReviewSummaryAsync(int productId);
    Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewDto dto, IFormFile? image);
    Task<ReviewDto> UpdateReviewAsync(int userId, int reviewId, CreateReviewDto dto, IFormFile? image);
    Task DeleteReviewAsync(int userId, int reviewId);
    Task<ReviewDto?> GetMyReviewForProductAsync(int userId, int productId);
    Task<List<ReviewDto>> GetMyReviewsAsync(int userId);
}