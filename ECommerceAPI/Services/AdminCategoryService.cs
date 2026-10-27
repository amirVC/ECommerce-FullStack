using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace ECommerceAPI.Services;

public class AdminCategoryService : IAdminCategoryService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    private const string AllCategoriesCacheKey = "admin_categories_all";
    private static readonly TimeSpan CacheDuration =
        TimeSpan.FromMinutes(10);

    public AdminCategoryService(
        AppDbContext context,
        IMemoryCache cache)
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
                Name = c.Name,
                Slug = c.Slug
            })
            .ToListAsync();

        _cache.Set(
            AllCategoriesCacheKey,
            categories,
            CacheDuration);

        return categories;
    }

    public async Task<AdminCategoryResponseDto?> GetBySlugAsync(
        string slug)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug);

        if (category == null)
            return null;

        return new AdminCategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug
        };
    }


    public async Task<AdminCategoryResponseDto> CreateAsync(
        AdminCreateCategoryDto dto)
    {
        var name = dto.Name.Trim();
        var slug = dto.Slug.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Category name is required.");

        if (!IsValidSlug(slug))
            throw new BadRequestException(
                "Slug must contain lowercase letters, numbers, and single hyphens only.");

        if (await _context.Categories.AnyAsync(c => c.Name == name))
            throw new BadRequestException("Category already exists.");

        if (await _context.Categories.AnyAsync(c => c.Slug == slug))
            throw new BadRequestException(
                "This slug is already in use. Choose another slug.");

        var category = new Category
        {
            Name = name,
            Slug = slug
        };

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        _cache.Remove(AllCategoriesCacheKey);

        return new AdminCategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug
        };
    }


    public async Task<AdminCategoryResponseDto?> UpdateAsync(
        string slug,
        AdminUpdateCategoryDto dto)
    {
        // Find the category using its ORIGINAL slug.
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug);

        if (category == null)
            throw new NotFoundException("Category not found.");

        var name = dto.Name.Trim();
        var newSlug = dto.Slug.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Category name is required.");

        if (!IsValidSlug(newSlug))
            throw new BadRequestException(
                "Slug must contain lowercase letters, numbers, and single hyphens only.");

        var duplicateName = await _context.Categories
            .AnyAsync(c => c.Id != category.Id && c.Name == name);

        if (duplicateName)
            throw new BadRequestException("Category already exists.");

        var duplicateSlug = await _context.Categories
            .AnyAsync(c => c.Id != category.Id && c.Slug == newSlug);

        if (duplicateSlug)
            throw new BadRequestException(
                "This slug is already in use. Choose another slug.");

        // Save both fields independently.
        category.Name = name;
        category.Slug = newSlug;

        await _context.SaveChangesAsync();

        _cache.Remove(AllCategoriesCacheKey);

        return new AdminCategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug
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

    private async Task<string> GenerateUniqueSlugAsync(
        string name,
        int? excludeCategoryId = null)
    {
        var baseSlug = CreateSlug(name);

        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = "category";

        var candidate = baseSlug;
        var suffix = 2;

        while (await _context.Categories.AnyAsync(c =>
                   c.Slug == candidate &&
                   (!excludeCategoryId.HasValue ||
                    c.Id != excludeCategoryId.Value)))
        {
            candidate = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return candidate;
    }

    private static string CreateSlug(string value)
    {
        var normalized = value
            .Trim()
            .Normalize(NormalizationForm.FormKC)
            .ToLowerInvariant();

        var result = new StringBuilder();
        var previousWasSeparator = true;

        foreach (var character in normalized)
        {
            if (char.IsLetterOrDigit(character))
            {
                result.Append(character);
                previousWasSeparator = false;
            }
            else if (!previousWasSeparator)
            {
                result.Append('-');
                previousWasSeparator = true;
            }
        }

        return result.ToString().Trim('-');
    }


    private static bool IsValidSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > 150)
            return false;

        if (slug[0] == '-' || slug[^1] == '-')
            return false;

        var previousWasHyphen = false;

        foreach (var character in slug)
        {
            if (character == '-')
            {
                if (previousWasHyphen)
                    return false;

                previousWasHyphen = true;
            }
            else if (character >= 'a' && character <= 'z' ||
                     character >= '0' && character <= '9')
            {
                previousWasHyphen = false;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

}