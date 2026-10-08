using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IPaymentService
    {
        // Creates (or reuses) a PaymentIntent for the given order, owned by the given user.
        Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(int orderId, int userId);

        // Verifies the Stripe webhook signature and updates Payment/Order state accordingly.
        // Returns true if the event was handled, false if it was ignored (unrecognized event type).
        Task<bool> HandleWebhookAsync(string json, string stripeSignatureHeader);

        // Full or partial refund. Amount == null means refund the full remaining amount.
        Task<RefundResponseDto> RefundAsync(int orderId, decimal? amount, string? reason);

        // Payment history for a given user (their own orders only).
        Task<List<PaymentDto>> GetHistoryForUserAsync(int userId);

        // Single order's payment history (used by admin order details + user order details).
        Task<List<PaymentDto>> GetHistoryForOrderAsync(int orderId);
    }
}

