using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;

    public ReviewService(AppDbContext context, IImageService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    public async Task<PagedResultDto<ReviewDto>> GetProductReviewsAsync(int productId, int page, int pageSize)
    {
        var query = _context.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserId = r.UserId,
                UserName = r.User != null ? r.User.FullName : "Anonymous",
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                ImageUrl = r.ImageUrl,
                ThumbnailUrl = r.ThumbnailUrl,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PagedResultDto<ReviewDto>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<ReviewSummaryDto> GetProductReviewSummaryAsync(int productId)
    {
        var stats = await _context.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Average = g.Average(r => (double)r.Rating),
                FiveStar = g.Count(r => r.Rating == 5),
                FourStar = g.Count(r => r.Rating == 4),
                ThreeStar = g.Count(r => r.Rating == 3),
                TwoStar = g.Count(r => r.Rating == 2),
                OneStar = g.Count(r => r.Rating == 1)
            })
            .FirstOrDefaultAsync();

        if (stats == null)
        {
            return new ReviewSummaryDto
            {
                TotalReviews = 0,
                AverageRating = 0,
                RatingBreakdown = new Dictionary<int, int>
                {
                    [5] = 0,
                    [4] = 0,
                    [3] = 0,
                    [2] = 0,
                    [1] = 0
                }
            };
        }

        return new ReviewSummaryDto
        {
            TotalReviews = stats.Total,
            AverageRating = Math.Round(stats.Average, 1),
            RatingBreakdown = new Dictionary<int, int>
            {
                [5] = stats.FiveStar,
                [4] = stats.FourStar,
                [3] = stats.ThreeStar,
                [2] = stats.TwoStar,
                [1] = stats.OneStar
            }
        };
    }

    public async Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewDto dto, IFormFile? image)
    {
        if (dto.Rating < 1 || dto.Rating > 5)
            throw new BadRequestException("Rating must be between 1 and 5.");

        var alreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);
        if (alreadyReviewed)
            throw new BadRequestException("You already reviewed this product.");

        var productExists = await _context.Products.AnyAsync(p => p.Id == dto.ProductId);
        if (!productExists)
            throw new NotFoundException("Product not found.");

        string? imageUrl = null;
        string? thumbnailUrl = null;
        if (image != null)
        {
            (imageUrl, thumbnailUrl) = await _imageService.SaveReviewImageAsync(image);
        }

        var isVerified = await _context.OrderItems
            .AnyAsync(oi => oi.ProductId == dto.ProductId
                && oi.Order.UserId == userId
                && oi.Order.Status.ToLower() != "cancelled");

        var review = new Review
        {
            ProductId = dto.ProductId,
            UserId = userId,
            Rating = dto.Rating,
            Title = dto.Title,
            Comment = dto.Comment,
            ImageUrl = imageUrl,
            ThumbnailUrl = thumbnailUrl,
            IsVerifiedPurchase = isVerified,   // now real
            Status = ReviewStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var userName = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.FullName)
            .FirstOrDefaultAsync() ?? "Anonymous";

        return new ReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            UserId = review.UserId,
            UserName = userName,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            ImageUrl = review.ImageUrl,
            ThumbnailUrl = review.ThumbnailUrl,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<ReviewDto> UpdateReviewAsync(int userId, int reviewId, CreateReviewDto dto, IFormFile? image)
    {
        var review = await _context.Reviews.Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == reviewId)
            ?? throw new NotFoundException("Review not found.");

        if (review.UserId != userId)
            throw new UnauthorizedException("You can only edit your own review.");

        if (dto.Rating < 1 || dto.Rating > 5)
            throw new BadRequestException("Rating must be between 1 and 5.");

        review.Rating = dto.Rating;
        review.Title = dto.Title;
        review.Comment = dto.Comment;
        review.UpdatedAt = DateTime.UtcNow;

        if (image != null)
        {
            _imageService.DeleteReviewImage(review.ImageUrl);
            _imageService.DeleteReviewThumbnail(review.ThumbnailUrl);

            var (imageUrl, thumbnailUrl) = await _imageService.SaveReviewImageAsync(image);
            review.ImageUrl = imageUrl;
            review.ThumbnailUrl = thumbnailUrl;
        }

        await _context.SaveChangesAsync();
        return MapToDto(review);
    }

    public async Task DeleteReviewAsync(int userId, int reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId)
            ?? throw new NotFoundException("Review not found.");

        if (review.UserId != userId)
            throw new UnauthorizedException("You can only delete your own review.");

        _imageService.DeleteReviewImage(review.ImageUrl);
        _imageService.DeleteReviewThumbnail(review.ThumbnailUrl);

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }

    private static ReviewDto MapToDto(Review r) => new()
    {
        Id = r.Id,
        ProductId = r.ProductId,
        UserId = r.UserId,
        UserName = r.User?.FullName ?? "Anonymous", // your User model uses FullName, not UserName
        Rating = r.Rating,
        Title = r.Title,
        Comment = r.Comment,
        ImageUrl = r.ImageUrl,
        ThumbnailUrl = r.ThumbnailUrl,
        IsVerifiedPurchase = r.IsVerifiedPurchase,
        CreatedAt = r.CreatedAt
    };

    public async Task<ReviewDto?> GetMyReviewForProductAsync(int userId, int productId)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.UserId == userId)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserId = r.UserId,
                UserName = r.User != null ? r.User.FullName : "Anonymous",
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                ImageUrl = r.ImageUrl,
                ThumbnailUrl = r.ThumbnailUrl,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<ReviewDto>> GetMyReviewsAsync(int userId)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                UserId = r.UserId,
                UserName = r.User.FullName,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                ImageUrl = r.ImageUrl,
                ThumbnailUrl = r.ThumbnailUrl,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }
}