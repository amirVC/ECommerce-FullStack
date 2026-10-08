using ECommerceAPI.Models;

namespace ECommerceAPI.DTOs
{
    // Customer submits this to request a refund.
    public class CreateRefundRequestDto
    {
        public int OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    // Shown to the customer on their own order/requests.
    public class RefundRequestDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public RefundRequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? AdminNote { get; set; }
    }

    // Shown to admins, includes customer/order context.
    public class AdminRefundRequestDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
        public string Reason { get; set; } = string.Empty;
        public RefundRequestStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? AdminNote { get; set; }
    }

    // Admin approves or rejects with an optional note.
    public class ReviewRefundRequestDto
    {
        public string? AdminNote { get; set; }
    }
}
