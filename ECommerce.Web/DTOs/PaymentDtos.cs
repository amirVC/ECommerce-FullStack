namespace ECommerce.Web.DTOs
{
    public class CreatePaymentIntentDto
    {
        public int OrderId { get; set; }
    }

    public class PaymentIntentResponseDto
    {
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    // DirectRefundDto / RefundResponseDto already live in RefundDtos.cs (RefundResponseDto.Status
    // is a plain string there, not PaymentStatus) — not redefined here.

    // These two enums must mirror ECommerceAPI.Models.PaymentProvider / PaymentStatus
    // EXACTLY (same names, same order) since the API serializes them as plain ints
    // over JSON and ECommerce.Web doesn't reference ECommerceAPI's assembly.
    public enum PaymentProvider
    {
        Stripe = 0,
        PayPal = 1
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Succeeded = 1,
        Failed = 2,
        Refunded = 3,
        PartiallyRefunded = 4,
        Canceled = 5
    }

    // Payment history row for the customer's own "My Payments" page.
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

    // Payment row for the admin Payments page - same as PaymentDto plus who it belongs to.
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

    public class AdminPaymentHistoryResultDto
    {
        public List<AdminPaymentDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    // Filters used by PaymentService.GetAllForAdminAsync() to build the query string
    // sent to GET api/admin/payments.
    public class AdminPaymentQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public PaymentStatus? Status { get; set; }
        public PaymentProvider? Provider { get; set; }
        public int? OrderId { get; set; }
        public int? UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
