using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IAddressService
{
    Task<List<AddressDto>> GetMyAddressesAsync();
    Task<AddressDto?> CreateAsync(CreateAddressDto dto);
    Task<AddressDto?> UpdateAsync(int id, UpdateAddressDto dto);
    Task DeleteAsync(int id);
    Task SetDefaultAsync(int id);
}
