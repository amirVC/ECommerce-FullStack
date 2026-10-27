using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ECommerce.Web.Common;
using ECommerce.Web.DTOs;
using ECommerce.Web.Exceptions;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Base;
using ECommerce.Web.ViewModels;

namespace ECommerce.Web.Services;

public class AdminProductService : ApiServiceBase, IAdminProductService
{
    public AdminProductService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, httpContextAccessor)
    {
    }

    public async Task<List<AdminProductDto>> GetAllAsync()
    {
        var client = CreateClient();

        var response = await client.GetAsync("api/admin/products");

        response.EnsureSuccessStatusCode();

        return await DeserializeAsync<List<AdminProductDto>>(response)
               ?? new();
    }

    public async Task<AdminProductDto> CreateAsync(AdminCreateProductDto dto, IFormFile? imageFile)
    {
        var client = CreateClient();

        using var content = BuildMultipartContent(
            dto.SKU,
            dto.Name,
            dto.Slug,
            dto.Description,
            dto.Price,
            dto.Stock,
            dto.CategoryId,
            dto.ImageUrl,
            dto.ImageAltText,
            dto.SalePrice,
            dto.SaleStartDate,
            dto.SaleEndDate,
            imageFile);

        var response = await client.PostAsync("api/admin/products", content);


        response.EnsureSuccessStatusCode();

        return (await DeserializeAsync<AdminProductDto>(response))!;
    }
    public async Task<AdminProductDto> UpdateAsync(
        string slug,
        AdminUpdateProductDto dto,
        IFormFile? imageFile)
    {
        var client = CreateClient();

        using var content = BuildMultipartContent(
            dto.SKU,
            dto.Name,
            dto.Slug,
            dto.Description,
            dto.Price,
            dto.Stock,
            dto.CategoryId,
            dto.ImageUrl,
            dto.ImageAltText,
            dto.SalePrice,
            dto.SaleStartDate,
            dto.SaleEndDate,
            imageFile);

        var response = await client.PutAsync(
            $"api/admin/products/by-slug/{Uri.EscapeDataString(slug)}",
            content);
        await EnsureSuccessOrThrowApiAsync(response);

        response.EnsureSuccessStatusCode();

        return (await DeserializeAsync<AdminProductDto>(response))!;
    }



    public async Task DeleteAsync(int id)
    {
        var client = CreateClient();

        var response = await client.DeleteAsync($"api/admin/products/{id}");

        response.EnsureSuccessStatusCode();
    }

    public async Task<AdminUpdateProductDto?> GetByIdAsync(int id)
    {
        var client = CreateClient();

        var response = await client.GetAsync($"api/admin/products/{id}");

        if (!response.IsSuccessStatusCode)
            return null;

        var dto = await DeserializeAsync<AdminUpdateProductDto>(response);

        if (dto != null)
        {
            dto.SaleStartDate = ToLocalForDisplay(dto.SaleStartDate);
            dto.SaleEndDate = ToLocalForDisplay(dto.SaleEndDate);
        }

        return dto;
    }


    public async Task<AdminUpdateProductDto?> GetBySlugAsync(string slug)
    {
        var client = CreateClient();

        var response = await client.GetAsync(
            $"api/admin/products/by-slug/{Uri.EscapeDataString(slug)}");

        if (!response.IsSuccessStatusCode)
            return null;

        var dto = await DeserializeAsync<AdminUpdateProductDto>(response);

        if (dto != null)
        {
            dto.SaleStartDate = ToLocalForDisplay(dto.SaleStartDate);
            dto.SaleEndDate = ToLocalForDisplay(dto.SaleEndDate);
        }

        return dto;
    }

    private async Task EnsureSuccessOrThrowApiAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var statusCode = (int)response.StatusCode;

        if (statusCode is 400 or 404)
        {
            var body = await ReadErrorAsync(response);
            var message = ExtractMessage(body) ?? "The request could not be completed.";

            throw new ApiException(message, statusCode);
        }

        response.EnsureSuccessStatusCode();
    }

    private static string? ExtractMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(body);

            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            foreach (var property in doc.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String &&
                    (property.NameEquals("message") ||
                     property.Name.Equals("message", StringComparison.OrdinalIgnoreCase) ||
                     property.Name.Equals("title", StringComparison.OrdinalIgnoreCase)))
                {
                    return property.Value.GetString();
                }
            }
        }
        catch (JsonException)
        {
          
        }

        return null;
    }
    private static DateTime? ToLocalForDisplay(DateTime? dt)
    {
        if (!dt.HasValue) return null;

        return dt.Value.Kind == DateTimeKind.Utc
            ? dt.Value.ToLocalTime()
            : dt.Value;
    }

    public async Task<PagedResultDto<AdminProductDto>> SearchAsync(ProductSearchDto dto)
    {
        var client = CreateClient();

        var url =
            $"api/admin/products/search?search={Uri.EscapeDataString(dto.Search ?? "")}" +
            $"&page={dto.Page}" +
            $"&pageSize={dto.PageSize}";

        var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();

        return await DeserializeAsync<PagedResultDto<AdminProductDto>>(response)
               ?? new();
    }

    private static MultipartFormDataContent BuildMultipartContent(
        string sku,
        string name,
        string? slug,
        string description,
        decimal price,
        int stock,
        int categoryId,
        string? imageUrl,
        string? imageAltText,
        decimal? salePrice,
        DateTime? saleStartDate,
        DateTime? saleEndDate,
        IFormFile? imageFile)
    {



        var content = new MultipartFormDataContent
    {
        { new StringContent(sku ?? ""), "SKU" },
        { new StringContent(name ?? ""), "Name" },
        { new StringContent(description ?? ""), "Description" },
        { new StringContent(price.ToString(CultureInfo.InvariantCulture)), "Price" },
        { new StringContent(stock.ToString(CultureInfo.InvariantCulture)), "Stock" },
        { new StringContent(categoryId.ToString(CultureInfo.InvariantCulture)), "CategoryId" }
    };

        if (!string.IsNullOrWhiteSpace(imageUrl))
        {
            content.Add(new StringContent(imageUrl), "ImageUrl");
        }
        if (!string.IsNullOrWhiteSpace(slug))
        {
            content.Add(new StringContent(slug.Trim()), "Slug");
        }

        if (!string.IsNullOrWhiteSpace(imageAltText))
        {
            content.Add(new StringContent(imageAltText), "ImageAltText");
        }

        if (salePrice.HasValue)
        {
            content.Add(new StringContent(salePrice.Value.ToString(CultureInfo.InvariantCulture)), "SalePrice");
        }

        if (saleStartDate.HasValue)
        {
            content.Add(new StringContent(saleStartDate.Value.ToString("o", CultureInfo.InvariantCulture)), "SaleStartDate");
        }

        if (saleEndDate.HasValue)
        {
            content.Add(new StringContent(saleEndDate.Value.ToString("o", CultureInfo.InvariantCulture)), "SaleEndDate");
        }

        if (imageFile != null && imageFile.Length > 0)
        {
            var streamContent = new StreamContent(imageFile.OpenReadStream());
            streamContent.Headers.ContentType =
                new MediaTypeHeaderValue(imageFile.ContentType);

            content.Add(streamContent, "ImageFile", imageFile.FileName);
        }

        return content;
    }

    public async Task<List<ProductImageViewModel>> GetImagesAsync(int productId)
    {
        var client = CreateClient();

        var response = await client.GetAsync(
            $"api/products/{productId}/images");

        response.EnsureSuccessStatusCode();

        return await DeserializeAsync<List<ProductImageViewModel>>(response)
               ?? new();
    }

    public async Task AddImageAsync(
        int productId,
        AdminCreateProductImageViewModel model)
    {
        var client = CreateClient();

        using var content = new MultipartFormDataContent();

        content.Add(
            new StreamContent(model.ImageFile.OpenReadStream()),
            "ImageFile",
            model.ImageFile.FileName);

        content.Add(
            new StringContent(model.AltText ?? ""),
            "AltText");

        var response = await client.PostAsync(
            $"api/products/{productId}/images",
            content);

        response.EnsureSuccessStatusCode();
    }
    public async Task DeleteImageAsync(int imageId)
    {
        var client = CreateClient();

        var response =
            await client.DeleteAsync($"api/product-images/{imageId}");

        response.EnsureSuccessStatusCode();
    }

    public async Task SetPrimaryImageAsync(int imageId)
    {
        var client = CreateClient();

        var response =
            await client.PutAsync(
                $"api/product-images/{imageId}/primary",
                null);

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateImageAltTextAsync(int imageId, string altText)
    {
        var client = CreateClient();

        var jsonContent = JsonSerializer.Serialize(new { altText = altText ?? string.Empty });
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await client.PutAsync($"api/product-images/{imageId}/alt-text", content);

        response.EnsureSuccessStatusCode();
    }

}