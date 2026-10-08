namespace ECommerce.Web.DTOs;

public class AdminCreateCouponDto
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string DiscountType { get; set; } = "Percentage";

    public decimal? DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    public string Scope { get; set; } = "Order";

    public decimal? MinimumOrderAmount { get; set; }
    public int? UsageLimitTotal { get; set; }
    public int? UsageLimitPerUser { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public List<int> ProductIds { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
}