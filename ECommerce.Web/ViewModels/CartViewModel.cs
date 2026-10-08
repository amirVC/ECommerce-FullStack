namespace ECommerce.Web.ViewModels;

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();

    // Original/main price before any discounts
    public decimal Subtotal { get; set; }

    // Sale discount
    public decimal SaleDiscountAmount { get; set; }

    // Campaign discount after sale price
    public decimal CampaignDiscountAmount { get; set; }

    // Coupon discount after sale + campaign
    public decimal DiscountAmount { get; set; }

    public string? AppliedCouponCode { get; set; }

    public bool FreeShippingApplied { get; set; }

    // Final amount after all discounts
    public decimal Total { get; set; }

    // Guests have no server-side Cart row to attach a coupon to.
    public bool CanApplyCoupon { get; set; }
}