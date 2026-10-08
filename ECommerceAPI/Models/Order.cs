namespace ECommerceAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Public API identifier. Keep Id internal for EF relationships.
        public Guid PublicId { get; set; } = Guid.NewGuid();

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Pending";

        public decimal TotalAmount { get; set; }

        // Customer Information
        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? State { get; set; }

        public string PostalCode { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string? Notes { get; set; }

        // Payment
        public string PaymentMethod { get; set; } = "Cash";

        public string PaymentStatus { get; set; } = "Pending";

        // Discounts
        public string? CouponCode { get; set; }

        public decimal DiscountAmount { get; set; } = 0m;

        // Campaign discount
        public decimal CampaignDiscountAmount { get; set; } = 0m;

        // Shipping
        public decimal ShippingCost { get; set; } = 0m;

        // Foreign Key
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public int? AddressId { get; set; }
        public Address? SavedAddress { get; set; }

        // Selected delivery method + its price snapshot lives in ShippingCost above
        public int? DeliveryMethodId { get; set; }
        public DeliveryMethod? DeliveryMethod { get; set; }

        // One shipment per order (tracking, carrier, status timeline)
        public Shipment? Shipment { get; set; }
    }
}