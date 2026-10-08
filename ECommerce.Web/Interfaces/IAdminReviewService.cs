using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IAdminReviewService
{
    Task<PagedResultDto<AdminReviewDto>> GetAllReviewsAsync(int page, int pageSize, string? status);
    Task UpdateStatusAsync(int reviewId, string status);
    Task DeleteReviewAsync(int reviewId);
}