namespace ECommerceAPI.DTOs;

public class CouponDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal? DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public string Scope { get; set; } = string.Empty;
    public decimal? MinimumOrderAmount { get; set; }
    public int? UsageLimitTotal { get; set; }
    public int? UsageLimitPerUser { get; set; }
    public int TimesUsed { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public List<int> ProductIds { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
}