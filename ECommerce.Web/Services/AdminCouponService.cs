using System.Text;
using System.Text.Json;
using ECommerce.Web.Common;
using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Base;

namespace ECommerce.Web.Services;

public class AdminCouponService : ApiServiceBase, IAdminCouponService
{
    public AdminCouponService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, httpContextAccessor)
    {
    }

    public async Task<PagedResultDto<CouponDto>> GetAllAsync(int page, int pageSize)
    {
        var client = CreateClient();

        var response = await client.GetAsync($"api/admin/coupons?page={page}&pageSize={pageSize}");

        response.EnsureSuccessStatusCode();

        return await DeserializeAsync<PagedResultDto<CouponDto>>(response)
               ?? new();
    }

    public async Task<CouponDto?> GetByIdAsync(int id)
    {
        var client = CreateClient();

        var response = await client.GetAsync($"api/admin/coupons/{id}");

        if (!response.IsSuccessStatusCode)
            return null;

        return await DeserializeAsync<CouponDto>(response);
    }

    public async Task<CouponDto> CreateAsync(AdminCreateCouponDto dto)
    {
        var client = CreateClient();

        var content = new StringContent(
            JsonSerializer.Serialize(dto),
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("api/admin/coupons", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(string.IsNullOrWhiteSpace(error) ? "Failed to create coupon." : error);
        }

        return (await DeserializeAsync<CouponDto>(response))!;
    }

    public async Task<CouponDto?> UpdateAsync(int id, AdminUpdateCouponDto dto)
    {
        var client = CreateClient();

        var content = new StringContent(
            JsonSerializer.Serialize(dto),
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync($"api/admin/coupons/{id}", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(string.IsNullOrWhiteSpace(error) ? "Failed to update coupon." : error);
        }

        return await DeserializeAsync<CouponDto>(response);
    }

    public async Task DeleteAsync(int id)
    {
        var client = CreateClient();

        var response = await client.DeleteAsync($"api/admin/coupons/{id}");

        response.EnsureSuccessStatusCode();
    }
}