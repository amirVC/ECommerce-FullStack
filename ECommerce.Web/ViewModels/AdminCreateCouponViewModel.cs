using ECommerce.Web.DTOs;

namespace ECommerce.Web.ViewModels;

public class AdminCreateCouponViewModel
{
    public AdminCreateCouponDto Coupon { get; set; } = new();
    public List<CategoryLookupDto> Categories { get; set; } = new();
    public List<AdminProductDto> Products { get; set; } = new();
}