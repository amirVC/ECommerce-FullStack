using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Http;

namespace ECommerce.Web.Services;

public class AdminCampaignService : IAdminCampaignService
{
    private readonly IApiClient _apiClient;

    public AdminCampaignService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<CampaignDto>> GetAllAsync()
    {
        return await _apiClient.GetAsync<List<CampaignDto>>(
            "api/admin/campaigns")
            ?? new List<CampaignDto>();
    }

    public async Task<CampaignDto?> GetByIdAsync(int id)
    {
        return await _apiClient.GetAsync<CampaignDto>(
            $"api/admin/campaigns/{id}");
    }

    public async Task<CampaignDto> CreateAsync(
        AdminCreateCampaignDto dto)
    {
        dto.StartDate = ToUtc(dto.StartDate);
        dto.EndDate = ToUtc(dto.EndDate);

        return await _apiClient.PostAsync<
            AdminCreateCampaignDto,
            CampaignDto>(
            "api/admin/campaigns",
            dto);
    }
    public async Task<CampaignDto?> UpdateAsync(
        int id,
        AdminUpdateCampaignDto dto)
    {
        dto.StartDate = ToUtc(dto.StartDate);
        dto.EndDate = ToUtc(dto.EndDate);

        return await _apiClient.PutAsync<
            AdminUpdateCampaignDto,
            CampaignDto>(
            $"api/admin/campaigns/{id}",
            dto);
    }
    public async Task DeleteAsync(int id)
    {
        await _apiClient.DeleteAsync<object>(
            $"api/admin/campaigns/{id}");
    }

    private static DateTime ToUtc(DateTime value)
    {
        return DateTime.SpecifyKind(
            value,
            DateTimeKind.Local
        ).ToUniversalTime();
    }

}