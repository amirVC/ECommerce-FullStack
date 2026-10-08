using System.ComponentModel.DataAnnotations;
using ECommerceAPI.Models;

namespace ECommerceAPI.DTOs;

public class AdminUpdateCouponDto
{
    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public CouponDiscountType DiscountType { get; set; }

    public decimal? DiscountValue { get; set; }
    public decimal? MaxDiscountAmount { get; set; }

    public CouponScope Scope { get; set; } = CouponScope.Order;

    public decimal? MinimumOrderAmount { get; set; }
    public int? UsageLimitTotal { get; set; }
    public int? UsageLimitPerUser { get; set; }

    [Required]
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public List<int> ProductIds { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
}