using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Http;

namespace ECommerce.Web.Services;

public class AddressService : IAddressService
{
    private const string ApiBase = "api/addresses";

    private readonly IApiClient _apiClient;

    public AddressService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<AddressDto>> GetMyAddressesAsync()
    {
        var addresses = await _apiClient.GetAsync<List<AddressDto>>(ApiBase);
        return addresses ?? new List<AddressDto>();
    }

    public async Task<AddressDto?> CreateAsync(CreateAddressDto dto)
    {
        return await _apiClient.PostAsync<CreateAddressDto, AddressDto>(ApiBase, dto);
    }

    public async Task<AddressDto?> UpdateAsync(int id, UpdateAddressDto dto)
    {
        return await _apiClient.PutAsync<UpdateAddressDto, AddressDto>($"{ApiBase}/{id}", dto);
    }

    public async Task DeleteAsync(int id)
    {
        await _apiClient.DeleteAsync<object>($"{ApiBase}/{id}");
    }

    public async Task SetDefaultAsync(int id)
    {
        await _apiClient.PostAsync($"{ApiBase}/{id}/default", new { });
    }
}
