using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IAdminCampaignService
{
    Task<List<CampaignDto>> GetAllAsync();

    Task<CampaignDto?> GetByIdAsync(int id);

    Task<CampaignDto> CreateAsync(AdminCreateCampaignDto dto);

    Task<CampaignDto?> UpdateAsync(
        int id,
        AdminUpdateCampaignDto dto);

    Task DeleteAsync(int id);
}