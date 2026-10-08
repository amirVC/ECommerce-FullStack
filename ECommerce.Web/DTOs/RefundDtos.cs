namespace ECommerce.Web.DTOs
{
    public class DirectRefundDto
    {
        public decimal? Amount { get; set; }
        public string? Reason { get; set; }
    }

    public class RefundResponseDto
    {
        public int PaymentId { get; set; }
        public decimal RefundedAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}