using ECommerceAPI.DTOs;
using ECommerceAPI.Models;

namespace ECommerceAPI.Services.Interfaces;

public interface ICouponService
{
    Task<PagedResultDto<CouponDto>> GetAllAsync(int page, int pageSize);
    Task<CouponDto?> GetByIdAsync(int id);
    Task<CouponDto> CreateAsync(AdminCreateCouponDto dto);
    Task<CouponDto?> UpdateAsync(int id, AdminUpdateCouponDto dto);
    Task<bool> DeleteAsync(int id);

    Task<CouponValidationResultDto> ValidateAsync(string code, int userId, Cart cart);
    Task RecordUsageAsync(string code, int userId, int orderId, decimal discountApplied);
}