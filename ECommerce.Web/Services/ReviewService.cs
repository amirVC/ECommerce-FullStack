using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Http;

namespace ECommerce.Web.Services;

public class ReviewService : IReviewService
{
    private readonly IApiClient _apiClient;

    public ReviewService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<PagedResultDto<ReviewDto>> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10)
    {
        var result = await _apiClient.GetAsync<PagedResultDto<ReviewDto>>(
            $"api/reviews/product/{productId}?page={page}&pageSize={pageSize}");

        return result ?? new PagedResultDto<ReviewDto>();
    }

    public async Task<ReviewSummaryDto> GetReviewSummaryAsync(int productId)
    {
        var result = await _apiClient.GetAsync<ReviewSummaryDto>(
            $"api/reviews/product/{productId}/summary");

        return result ?? new ReviewSummaryDto();
    }

    public async Task<ReviewDto?> CreateReviewAsync(CreateReviewDto dto, IFormFile? image)
    {
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(dto.ProductId.ToString()), "ProductId");
        content.Add(new StringContent(dto.Rating.ToString()), "Rating");
        content.Add(new StringContent(dto.Comment ?? string.Empty), "Comment");

        if (!string.IsNullOrWhiteSpace(dto.Title))
            content.Add(new StringContent(dto.Title), "Title");

        if (image != null)
        {
            var streamContent = new StreamContent(image.OpenReadStream());
            streamContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
            content.Add(streamContent, "image", image.FileName);
        }

        return await _apiClient.PostMultipartAsync<ReviewDto>("api/reviews", content);
    }

    public async Task DeleteReviewAsync(int reviewId)
    {
        await _apiClient.DeleteAsync<object>($"api/reviews/{reviewId}");
    }

    public async Task<ReviewDto?> GetMyReviewForProductAsync(int productId)
    {
        try
        {
            return await _apiClient.GetAsync<ReviewDto>($"api/reviews/product/{productId}/mine");
        }
        catch
        {
            return null; // 204 No Content or not logged in
        }
    }

    public async Task<ReviewDto?> UpdateReviewAsync(int reviewId, CreateReviewDto dto, IFormFile? image)
    {
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(dto.ProductId.ToString()), "ProductId");
        content.Add(new StringContent(dto.Rating.ToString()), "Rating");
        content.Add(new StringContent(dto.Comment ?? string.Empty), "Comment");

        if (!string.IsNullOrWhiteSpace(dto.Title))
            content.Add(new StringContent(dto.Title), "Title");

        if (image != null)
        {
            var streamContent = new StreamContent(image.OpenReadStream());
            streamContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
            content.Add(streamContent, "image", image.FileName);
        }

        return await _apiClient.PutMultipartAsync<ReviewDto>($"api/reviews/{reviewId}", content);
    }
    public async Task<List<ReviewDto>> GetMyReviewsAsync()
    {
        var result = await _apiClient.GetAsync<List<ReviewDto>>("api/reviews/mine");
        return result ?? new List<ReviewDto>();
    }
}