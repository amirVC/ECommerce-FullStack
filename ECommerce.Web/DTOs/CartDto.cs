namespace ECommerce.Web.DTOs;

public class CartDto
{
    public int Id { get; set; }

public List<CartItemDto> Items { get; set; } = new();

    public int TotalItems { get; set; }

    public decimal TotalAmount { get; set; }

    // Coupon / discount information
    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public string? AppliedCouponCode { get; set; }

    public bool FreeShippingApplied { get; set; }

    public decimal CampaignDiscountAmount { get; set; }

    public decimal SaleDiscountAmount { get; set; }
}
