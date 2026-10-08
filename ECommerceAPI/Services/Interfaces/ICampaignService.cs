using ECommerceAPI.DTOs;
using ECommerceAPI.Models;

namespace ECommerceAPI.Services.Interfaces;

public interface ICampaignService
{
    Task<List<CampaignDto>> GetAllAsync();

    Task<CampaignDto?> GetByIdAsync(int id);

    Task<CampaignDto> CreateAsync(AdminCreateCampaignDto dto);

    Task<CampaignDto?> UpdateAsync(
        int id,
        AdminUpdateCampaignDto dto);

    Task<bool> DeleteAsync(int id);

    Task<CampaignDto?> GetActiveCampaignForProductAsync(
        int productId);

    Task<decimal> CalculateDiscountAsync(
        Product product,
        decimal basePrice);

    Task<Dictionary<int, CampaignDto>>
        GetActiveCampaignsForProductsAsync(
            IEnumerable<(int ProductId, int CategoryId)> products);
}