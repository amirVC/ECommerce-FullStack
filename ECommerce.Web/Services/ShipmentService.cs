using ECommerce.Web.DTOs;
using ECommerce.Web.Exceptions;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Http;

namespace ECommerce.Web.Services;

public class ShipmentService : IShipmentService
{
    private readonly IApiClient _apiClient;

    public ShipmentService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<ShipmentDto?> GetByOrderIdAsync(int orderId)
    {
        try
        {
            return await _apiClient.GetAsync<ShipmentDto>($"api/orders/{orderId}/shipment");
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<ShipmentDto?> CreateForOrderAsync(int orderId, CreateShipmentDto dto)
    {
        return await _apiClient.PostAsync<CreateShipmentDto, ShipmentDto>(
            $"api/admin/orders/{orderId}/shipment", dto);
    }

    public async Task<ShipmentDto?> UpdateStatusAsync(int orderId, UpdateShipmentStatusDto dto)
    {
        return await _apiClient.PutAsync<UpdateShipmentStatusDto, ShipmentDto>(
            $"api/admin/orders/{orderId}/shipment/status", dto);
    }
}