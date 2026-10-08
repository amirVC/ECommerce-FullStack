using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces;

public interface IAdminReviewService
{
    Task<PagedResultDto<AdminReviewDto>> GetAllReviewsAsync(int page, int pageSize, string? status);
    Task<AdminReviewDto> UpdateStatusAsync(int reviewId, string status);
    Task DeleteReviewAsync(int reviewId);
}