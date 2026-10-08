using ECommerce.Web.Common;
using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IAdminCouponService
{
    Task<PagedResultDto<CouponDto>> GetAllAsync(int page, int pageSize);
    Task<CouponDto?> GetByIdAsync(int id);
    Task<CouponDto> CreateAsync(AdminCreateCouponDto dto);
    Task<CouponDto?> UpdateAsync(int id, AdminUpdateCouponDto dto);
    Task DeleteAsync(int id);
}