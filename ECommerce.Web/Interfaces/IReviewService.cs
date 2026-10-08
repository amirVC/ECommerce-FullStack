using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IReviewService
{
    Task<PagedResultDto<ReviewDto>> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10);
    Task<ReviewSummaryDto> GetReviewSummaryAsync(int productId);
    Task<ReviewDto?> CreateReviewAsync(CreateReviewDto dto, IFormFile? image);
    Task DeleteReviewAsync(int reviewId);
    Task<ReviewDto?> GetMyReviewForProductAsync(int productId);
    Task<ReviewDto?> UpdateReviewAsync(int reviewId, CreateReviewDto dto, IFormFile? image);
    Task<List<ReviewDto>> GetMyReviewsAsync();

}