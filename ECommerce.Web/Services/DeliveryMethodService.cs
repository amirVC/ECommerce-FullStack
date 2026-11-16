using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Http;

namespace ECommerce.Web.Services;

public class DeliveryMethodService : IDeliveryMethodService
{
    private const string PublicApiBase = "api/delivery-methods";
    private const string AdminApiBase = "api/admin/delivery-methods";

    private readonly IApiClient _apiClient;

    public DeliveryMethodService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<DeliveryMethodDto>> GetActiveAsync()
    {
        var methods = await _apiClient.GetAsync<List<DeliveryMethodDto>>(PublicApiBase);
        return methods ?? new List<DeliveryMethodDto>();
    }

    public async Task<List<DeliveryMethodDto>> GetAllAsync()
    {
        var methods = await _apiClient.GetAsync<List<DeliveryMethodDto>>(AdminApiBase);
        return methods ?? new List<DeliveryMethodDto>();
    }

    public async Task<DeliveryMethodDto?> GetByPublicIdAsync(Guid publicId)
    {
        return await _apiClient.GetAsync<DeliveryMethodDto>($"{AdminApiBase}/{publicId}");
    }

    public async Task<DeliveryMethodDto?> CreateAsync(CreateDeliveryMethodDto dto)
    {
        return await _apiClient.PostAsync<CreateDeliveryMethodDto, DeliveryMethodDto>(AdminApiBase, dto);
    }

    public async Task<DeliveryMethodDto?> UpdateAsync(Guid publicId, UpdateDeliveryMethodDto dto)
    {
        return await _apiClient.PutAsync<UpdateDeliveryMethodDto, DeliveryMethodDto>(
            $"{AdminApiBase}/{publicId}", dto);
    }

    public async Task DeleteAsync(Guid publicId)
    {
        await _apiClient.DeleteAsync<object>($"{AdminApiBase}/{publicId}");
    }
}