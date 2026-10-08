using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services;

public class AdminReviewService : IAdminReviewService
{
    private readonly AppDbContext _context;
    public AdminReviewService(AppDbContext context) => _context = context;

    public async Task<PagedResultDto<AdminReviewDto>> GetAllReviewsAsync(int page, int pageSize, string? status)
    {
        var query = _context.Reviews.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReviewStatus>(status, true, out var parsed))
            query = query.Where(r => r.Status == parsed);

        query = query.OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new AdminReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                UserId = r.UserId,
                UserName = r.User.FullName,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Comment,
                ImageUrl = r.ImageUrl,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                CreatedAt = r.CreatedAt,
                Status = r.Status.ToString()
            })
            .ToListAsync();

        return new PagedResultDto<AdminReviewDto> { Items = items, CurrentPage = page, PageSize = pageSize, TotalCount = total };
    }

    public async Task<AdminReviewDto> UpdateStatusAsync(int reviewId, string status)
    {
        var review = await _context.Reviews.Include(r => r.User).Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == reviewId)
            ?? throw new NotFoundException("Review not found.");

        if (!Enum.TryParse<ReviewStatus>(status, true, out var parsed))
            throw new BadRequestException("Invalid status. Use 'Approved' or 'Hidden'.");

        review.Status = parsed;
        await _context.SaveChangesAsync();

        return new AdminReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            ProductName = review.Product.Name,
            UserId = review.UserId,
            UserName = review.User.FullName,
            Rating = review.Rating,
            Title = review.Title,
            Comment = review.Comment,
            ImageUrl = review.ImageUrl,
            IsVerifiedPurchase = review.IsVerifiedPurchase,
            CreatedAt = review.CreatedAt,
            Status = review.Status.ToString()
        };
    }

    public async Task DeleteReviewAsync(int reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId)
            ?? throw new NotFoundException("Review not found.");
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
    }
}