using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Http;

namespace ECommerce.Web.Services;

public class AdminReviewService : IAdminReviewService
{
    private readonly IApiClient _apiClient;
    public AdminReviewService(IApiClient apiClient) => _apiClient = apiClient;

    public async Task<PagedResultDto<AdminReviewDto>> GetAllReviewsAsync(int page, int pageSize, string? status)
    {
        var url = $"api/admin/reviews?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(status))
            url += $"&status={status}";

        var result = await _apiClient.GetAsync<PagedResultDto<AdminReviewDto>>(url);
        return result ?? new PagedResultDto<AdminReviewDto>();
    }

    public async Task UpdateStatusAsync(int reviewId, string status)
    {
        await _apiClient.PostAsync<AdminUpdateReviewStatusDto, AdminReviewDto>(
            $"api/admin/reviews/{reviewId}/status",
            new AdminUpdateReviewStatusDto { Status = status });
    }

    public async Task DeleteReviewAsync(int reviewId)
    {
        await _apiClient.DeleteAsync<object>($"api/admin/reviews/{reviewId}");
    }
}