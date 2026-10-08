
using System.Text;
using System.Text.Json;
using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Base;

namespace ECommerce.Web.Services;

public class AdminCategoryService : ApiServiceBase, IAdminCategoryService
{
    public AdminCategoryService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, httpContextAccessor)
    {
    }

    public async Task<List<AdminCategoryDto>> GetAllAsync()
    {
        var client = CreateClient();

        var response = await client.GetAsync("api/admin/categories");

        response.EnsureSuccessStatusCode();

        return await DeserializeAsync<List<AdminCategoryDto>>(response)
               ?? new();
    }

    public async Task<AdminCategoryDto> CreateAsync(AdminCreateCategoryDto dto)
    {
        var client = CreateClient();

        var response = await client.PostAsync(
            "api/admin/categories",
            new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                "application/json"));

        response.EnsureSuccessStatusCode();

        return (await DeserializeAsync<AdminCategoryDto>(response))!;
    }

    public async Task<AdminCategoryDto?> GetByIdAsync(int id)
    {
        var client = CreateClient();

        var response = await client.GetAsync($"api/admin/categories/{id}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeAsync<AdminCategoryDto>(response);
    }

    public async Task<AdminCategoryDto> UpdateAsync(
        int id,
        AdminUpdateCategoryDto dto)
    {
        var client = CreateClient();

        var response = await client.PutAsync(
            $"api/admin/categories/{id}",
            new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                "application/json"));

        response.EnsureSuccessStatusCode();

        return (await DeserializeAsync<AdminCategoryDto>(response))!;
    }

    public async Task DeleteAsync(int id)
    {
        var client = CreateClient();

        var response = await client.DeleteAsync(
            $"api/admin/categories/{id}");

        response.EnsureSuccessStatusCode();
    }
}

