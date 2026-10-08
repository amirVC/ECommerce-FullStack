using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.Models
{
    public class RefundRequest
    {
        public int Id { get; set; }

        // Public API identifier.
        public Guid PublicId { get; set; } = Guid.NewGuid();

        [Required]
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public RefundRequestStatus Status { get; set; } = RefundRequestStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        [MaxLength(1000)]
        public string? AdminNote { get; set; }
    }
}
