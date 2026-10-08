using System.Security.Claims;
using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    // ==========================================
    // GET PRODUCT REVIEWS
    // Global limit: 100/minute
    // ==========================================
    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetProductReviews(
        int productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        return Ok(
            await _reviewService.GetProductReviewsAsync(
                productId,
                page,
                pageSize));
    }

    // ==========================================
    // GET REVIEW SUMMARY
    // Global limit: 100/minute
    // ==========================================
    [HttpGet("product/{productId}/summary")]
    public async Task<IActionResult> GetSummary(int productId)
    {
        return Ok(
            await _reviewService
                .GetProductReviewSummaryAsync(productId));
    }

    // ==========================================
    // CREATE REVIEW
    // Review limit: 10 / 10 minutes
    // ==========================================
    [HttpPost]
    [Authorize]
    [EnableRateLimiting("review")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] CreateReviewDto dto,
        [FromForm] IFormFile? image)
    {
        var userId =
            int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);

        var result =
            await _reviewService.CreateReviewAsync(
                userId,
                dto,
                image);

        return CreatedAtAction(
            nameof(GetProductReviews),
            new { productId = dto.ProductId },
            result);
    }

    // ==========================================
    // DELETE REVIEW
    // Global limit: 100/minute
    // ==========================================
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var userId =
            int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);

        await _reviewService.DeleteReviewAsync(
            userId,
            id);

        return NoContent();
    }

    // ==========================================
    // GET MY REVIEW FOR PRODUCT
    // Global limit: 100/minute
    // ==========================================
    [HttpGet("product/{productId}/mine")]
    [Authorize]
    public async Task<IActionResult> GetMyReview(
        int productId)
    {
        var userId =
            int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);

        var review =
            await _reviewService
                .GetMyReviewForProductAsync(
                    userId,
                    productId);

        return review == null
            ? NoContent()
            : Ok(review);
    }

    // ==========================================
    // GET MY REVIEWS
    // Global limit: 100/minute
    // ==========================================
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMyReviews()
    {
        var userId =
            int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);

        return Ok(
            await _reviewService.GetMyReviewsAsync(
                userId));
    }

    // ==========================================
    // UPDATE REVIEW
    // Review limit: 10 / 10 minutes
    // ==========================================
    [HttpPut("{id}")]
    [Authorize]
    [EnableRateLimiting("review")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(
        int id,
        [FromForm] CreateReviewDto dto,
        [FromForm] IFormFile? image)
    {
        var userId =
            int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);

        var result =
            await _reviewService.UpdateReviewAsync(
                userId,
                id,
                dto,
                image);

        return Ok(result);
    }
}
