using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Base;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Web.Services;

public class CategoryService : ApiServiceBase, ICategoryService
{
    private readonly IMemoryCache _cache;

    private const string AllCategoriesCacheKey = "web_categories_all";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public CategoryService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache cache)
        : base(httpClientFactory, httpContextAccessor)
    {
        _cache = cache;
    }

    public async Task<List<CategoryLookupDto>> GetAllAsync()
    {
        if (_cache.TryGetValue(
                AllCategoriesCacheKey,
                out List<CategoryLookupDto>? cached))
        {
            return cached!;
        }

        var client = CreateClient();

        var response = await client.GetAsync("api/admin/categories");

        response.EnsureSuccessStatusCode();

        var categories = await DeserializeAsync<List<CategoryLookupDto>>(response)
               ?? new();

        _cache.Set(AllCategoriesCacheKey, categories, CacheDuration);

        return categories;
    }
}