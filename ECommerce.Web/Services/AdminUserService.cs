using System.Text;
using System.Text.Json;
using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Base;

namespace ECommerce.Web.Services;

public class AdminUserService : ApiServiceBase, IAdminUserService
{
    public AdminUserService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, httpContextAccessor)
    {
    }

    public async Task<List<AdminUserDto>> GetAllAsync()
    {
        var client = CreateClient();

        var response = await client.GetAsync("api/admin/users");

        response.EnsureSuccessStatusCode();

        return await DeserializeAsync<List<AdminUserDto>>(response)
               ?? new();
    }

    public async Task<AdminUserDetailsDto?> GetByIdAsync(int id)
    {
        var client = CreateClient();

        var response = await client.GetAsync($"api/admin/users/{id}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeAsync<AdminUserDetailsDto>(response);
    }

    public async Task UpdateRoleAsync(
        int id,
        UpdateUserRoleDto dto)
    {
        var client = CreateClient();

        var response = await client.PutAsync(
            $"api/admin/users/{id}/role",
            new StringContent(
                JsonSerializer.Serialize(dto),
                Encoding.UTF8,
                "application/json"));

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var client = CreateClient();

        var response = await client.DeleteAsync($"api/admin/users/{id}");

        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadErrorAsync(response);

            throw new Exception(error);
        }
    }
}
