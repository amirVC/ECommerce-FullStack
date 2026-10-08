namespace ECommerceAPI.Models;

public class CouponUsage
{
    public int Id { get; set; }
    public int CouponId { get; set; }
    public Coupon Coupon { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int? OrderId { get; set; }
    public Order? Order { get; set; }
    public decimal DiscountAmountApplied { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
}