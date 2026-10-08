namespace ECommerceAPI.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public int TotalItems { get; set; }

        // Sum of Product.Price * qty — the "main price" line, before any discount.
        public decimal OriginalSubtotal { get; set; }

        // Sum of (Price - SalePrice) * qty — the "sale" discount line.
        public decimal SaleDiscountAmount { get; set; }

        // Sum of per-item campaign discounts (applied on top of sale price).
        public decimal CampaignDiscountAmount { get; set; }

        // Coupon discount, calculated on (OriginalSubtotal - SaleDiscountAmount - CampaignDiscountAmount).
        public decimal CouponDiscountAmount { get; set; }

        public string? AppliedCouponCode { get; set; }
        public bool FreeShippingApplied { get; set; }

        // Kept for backward compatibility with any existing callers/views.
        // Equal to CouponDiscountAmount.
        public decimal DiscountAmount { get; set; }

        // Post-sale, post-campaign, pre-coupon total. (i.e. what the coupon % is based on)
        public decimal Subtotal { get; set; }

        // OriginalSubtotal - SaleDiscountAmount - CampaignDiscountAmount - CouponDiscountAmount, floored at 0.
        public decimal TotalAmount { get; set; }
    }
}
