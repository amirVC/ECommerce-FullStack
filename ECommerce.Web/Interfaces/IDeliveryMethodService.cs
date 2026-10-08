using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IDeliveryMethodService
{
    // Public — used at checkout
    Task<List<DeliveryMethodDto>> GetActiveAsync();

    // Admin
    Task<List<DeliveryMethodDto>> GetAllAsync();
    Task<DeliveryMethodDto?> GetByIdAsync(int id);
    Task<DeliveryMethodDto?> CreateAsync(CreateDeliveryMethodDto dto);
    Task<DeliveryMethodDto?> UpdateAsync(int id, UpdateDeliveryMethodDto dto);
    Task DeleteAsync(int id);
}
