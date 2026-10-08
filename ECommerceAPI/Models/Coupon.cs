namespace ECommerceAPI.Models;

public class Coupon
{
    public int Id { get; set; }

    // Public API identifier.
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    public CouponDiscountType DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    public CouponScope Scope { get; set; } = CouponScope.Order;

    public decimal? MinimumOrderAmount { get; set; }
    public int? UsageLimitTotal { get; set; }
    public int? UsageLimitPerUser { get; set; }
    public int TimesUsed { get; set; } = 0;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CouponProduct> CouponProducts { get; set; } = new List<CouponProduct>();
    public ICollection<CouponCategory> CouponCategories { get; set; } = new List<CouponCategory>();
    public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();
}