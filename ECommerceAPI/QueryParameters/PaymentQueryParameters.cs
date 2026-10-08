using ECommerceAPI.Models;

namespace ECommerceAPI.QueryParameters
{
    public class PaymentQueryParameters
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public PaymentStatus? Status { get; set; }
        public PaymentProvider? Provider { get; set; }
        public int? OrderId { get; set; }
        public int? UserId { get; set; }

        // Inclusive on both ends; ToDate is treated as "through end of that day" in the service.
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
