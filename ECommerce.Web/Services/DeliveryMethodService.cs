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

    public async Task<DeliveryMethodDto?> GetByIdAsync(int id)
    {
        return await _apiClient.GetAsync<DeliveryMethodDto>($"{AdminApiBase}/{id}");
    }

    public async Task<DeliveryMethodDto?> CreateAsync(CreateDeliveryMethodDto dto)
    {
        return await _apiClient.PostAsync<CreateDeliveryMethodDto, DeliveryMethodDto>(AdminApiBase, dto);
    }

    public async Task<DeliveryMethodDto?> UpdateAsync(int id, UpdateDeliveryMethodDto dto)
    {
        return await _apiClient.PutAsync<UpdateDeliveryMethodDto, DeliveryMethodDto>($"{AdminApiBase}/{id}", dto);
    }

    public async Task DeleteAsync(int id)
    {
        await _apiClient.DeleteAsync<object>($"{AdminApiBase}/{id}");
    }
}
