using ECommerce.Web.Common;
using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Base;
using ECommerce.Web.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.Web.Services;

public class ProductService : ApiServiceBase, IProductService
{
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan ListCacheDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DetailsCacheDuration = TimeSpan.FromSeconds(60);

    public ProductService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache cache)
        : base(httpClientFactory, httpContextAccessor)
    {
        _cache = cache;
    }

    public async Task<PagedResult<ProductViewModel>> GetProductsAsync(
        int page = 1,
        string? search = null,
        int? categoryId = null,
        string? sortBy = null)
    {
        var cacheKey =
            $"products:list:{page}:{search}:{categoryId}:{sortBy}"; 

        if (_cache.TryGetValue(cacheKey, out PagedResult<ProductViewModel>? cached))
            return cached!;

        var client = CreateClient();

        var query = new List<string>
        {
            $"page={page}"
        };

        if (!string.IsNullOrWhiteSpace(search))
            query.Add($"search={Uri.EscapeDataString(search)}");

        if (categoryId.HasValue)
            query.Add($"categoryId={categoryId.Value}");

        if (!string.IsNullOrWhiteSpace(sortBy))
            query.Add($"sortBy={sortBy}");

        var url = $"api/products?{string.Join("&", query)}";

        var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var result = await DeserializeAsync<PagedResult<ProductViewModel>>(response)
               ?? new();

        _cache.Set(cacheKey, result, ListCacheDuration);

        return result;
    }

    public async Task<ProductViewModel?> GetProductByIdAsync(int id)
    {
        var cacheKey = $"products:details:{id}";

        if (_cache.TryGetValue(cacheKey, out ProductViewModel? cached))
            return cached;

        var client = CreateClient();

        var response = await client.GetAsync($"api/products/{id}");

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await DeserializeAsync<ProductViewModel>(response);

        if (result != null)
            _cache.Set(cacheKey, result, DetailsCacheDuration);

        return result;
    }

    public async Task<List<ProductViewModel>> GetRelatedProductsAsync(int productId)
    {
        var cacheKey = $"products:related:{productId}";

        if (_cache.TryGetValue(cacheKey, out List<ProductViewModel>? cached))
            return cached!;

        var client = CreateClient();
        var response = await client.GetAsync($"api/products/{productId}/related");

        response.EnsureSuccessStatusCode();

        // Deserialize to List<ProductViewModel> instead of AdminProductDto
        var result = await DeserializeAsync<List<ProductViewModel>>(response) ?? new();

        _cache.Set(cacheKey, result, DetailsCacheDuration);

        return result;
    }
}