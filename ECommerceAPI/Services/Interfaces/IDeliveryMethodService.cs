using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IDeliveryMethodService
    {
        Task<List<DeliveryMethodDto>> GetActiveAsync();
        Task<List<DeliveryMethodDto>> GetAllAsync();               // admin
        Task<DeliveryMethodDto> CreateAsync(CreateDeliveryMethodDto dto);
        Task<DeliveryMethodDto> UpdateAsync(int id, UpdateDeliveryMethodDto dto);
        Task DeleteAsync(int id);
    }
}
