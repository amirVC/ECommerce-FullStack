using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IAddressService
    {
        Task<List<AddressDto>> GetMyAddressesAsync(int userId);
        Task<AddressDto> CreateAsync(int userId, CreateAddressDto dto);
        Task<AddressDto> UpdateAsync(int userId, int addressId, UpdateAddressDto dto);
        Task DeleteAsync(int userId, int addressId);
        Task SetDefaultAsync(int userId, int addressId);
    }
}
