namespace ECommerceAPI.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Coupon
        public string? AppliedCouponCode { get; set; }

        public decimal DiscountAmount { get; set; } = 0m;

        public bool FreeShippingApplied { get; set; } = false;

        // Campaign
        public decimal CampaignDiscountAmount { get; set; } = 0m;

        // Navigation
        public ICollection<CartItem> CartItems { get; set; }
            = new List<CartItem>();
    }
}