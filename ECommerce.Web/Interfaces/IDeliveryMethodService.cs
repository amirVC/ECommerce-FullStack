using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IDeliveryMethodService
{

    Task<List<DeliveryMethodDto>> GetActiveAsync();


    Task<List<DeliveryMethodDto>> GetAllAsync();

    Task<DeliveryMethodDto?> GetByPublicIdAsync(Guid publicId);

    Task<DeliveryMethodDto?> CreateAsync(CreateDeliveryMethodDto dto);

    Task<DeliveryMethodDto?> UpdateAsync(
        Guid publicId,
        UpdateDeliveryMethodDto dto);

    Task DeleteAsync(Guid publicId);
}