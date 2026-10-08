using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceAPI.Models
{
    // NOTE: Adjust OrderId type / Order navigation to match your existing Order.cs (int vs Guid Id).
    public class Payment
    {
        public int Id { get; set; }

        // Public API identifier.
        public Guid PublicId { get; set; } = Guid.NewGuid();

        [Required]
        public int OrderId { get; set; }

        public Order? Order { get; set; }

        [Required]
        public PaymentProvider Provider { get; set; } = PaymentProvider.Stripe;

        // Stripe's PaymentIntent id (pi_...). Nullable for other providers.
        [MaxLength(255)]
        public string? PaymentIntentId { get; set; }

        // Stripe's client secret is NOT stored server-side long-term for security;
        // it's returned once at creation time and not persisted here.

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "usd";

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // How much has been refunded so far (supports partial refunds).
        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundedAmount { get; set; } = 0;

        [MaxLength(500)]
        public string? FailureReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
