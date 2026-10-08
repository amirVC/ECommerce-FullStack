using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerceAPI.Services;

public class AdminCategoryService : IAdminCategoryService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    private const string AllCategoriesCacheKey = "admin_categories_all";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public AdminCategoryService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IEnumerable<AdminCategoryResponseDto>> GetAllAsync()
    {
        if (_cache.TryGetValue(
                AllCategoriesCacheKey,
                out IEnumerable<AdminCategoryResponseDto>? cached))
        {
            return cached!;
        }

        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new AdminCategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync();

        _cache.Set(AllCategoriesCacheKey, categories, CacheDuration);

        return categories;
    }

    public async Task<AdminCategoryResponseDto?> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return null;

        return new AdminCategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public async Task<AdminCategoryResponseDto> CreateAsync(
        AdminCreateCategoryDto dto)
    {
        var exists = await _context.Categories
            .AnyAsync(c => c.Name == dto.Name);

        if (exists)
            throw new BadRequestException("Category already exists.");

        var category = new Category
        {
            Name = dto.Name
        };

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        _cache.Remove(AllCategoriesCacheKey);

        return new AdminCategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public async Task<AdminCategoryResponseDto> UpdateAsync(
        int id,
        AdminUpdateCategoryDto dto)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            throw new NotFoundException("Category not found.");

        category.Name = dto.Name;

        await _context.SaveChangesAsync();

        _cache.Remove(AllCategoriesCacheKey);

        return new AdminCategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            throw new NotFoundException("Category not found.");

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();

        _cache.Remove(AllCategoriesCacheKey);
    }
}