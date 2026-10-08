using ECommerceAPI.Models;

namespace ECommerceAPI.DTOs
{
    // Request: client asks the API to create a Stripe PaymentIntent for an order
    public class CreatePaymentIntentDto
    {
        public int OrderId { get; set; }
    }

    // Response: what the frontend needs to run Stripe.js / Stripe Elements
    public class PaymentIntentResponseDto
    {
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    // Payment history row shown to the customer
    public class PaymentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public PaymentProvider Provider { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public decimal RefundedAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DirectRefundDto
    {
        public decimal? Amount { get; set; }
        public string? Reason { get; set; }
    }

    public class RefundResponseDto
    {
        public int PaymentId { get; set; }
        public decimal RefundedAmount { get; set; }
        public PaymentStatus Status { get; set; }
    }

    // Payment row shown to admins — same as PaymentDto plus who it belongs to.
    public class AdminPaymentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public PaymentProvider Provider { get; set; }
        public string? PaymentIntentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public decimal RefundedAmount { get; set; }
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // NOTE: if your DTOs/PagedResultDto.cs already has a generic PagedResultDto<T> with
    // Items/TotalCount/Page/PageSize, feel free to swap this out for that instead — this
    // is self-contained so it doesn't assume that type's exact shape.
    public class AdminPaymentHistoryResultDto
    {
        public List<AdminPaymentDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}