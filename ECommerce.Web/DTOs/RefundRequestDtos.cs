namespace ECommerce.Web.DTOs
{
    public class CreateRefundRequestDto
    {
        public int OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class RefundRequestDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Pending" | "Approved" | "Rejected"
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? AdminNote { get; set; }
    }

    public class AdminRefundRequestDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? AdminNote { get; set; }
    }

    public class ReviewRefundRequestDto
    {
        public string? AdminNote { get; set; }
    }
}