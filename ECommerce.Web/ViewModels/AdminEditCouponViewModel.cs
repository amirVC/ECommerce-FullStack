using ECommerce.Web.DTOs;

namespace ECommerce.Web.ViewModels;

public class AdminEditCouponViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty; // display-only, not editable
    public int TimesUsed { get; set; }
    public AdminUpdateCouponDto Coupon { get; set; } = new();
    public List<CategoryLookupDto> Categories { get; set; } = new();
    public List<AdminProductDto> Products { get; set; } = new();
}