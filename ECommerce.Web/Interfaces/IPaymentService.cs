using ECommerce.Web.DTOs;
namespace ECommerce.Web.Interfaces;
public interface IPaymentService
{
    Task<PaymentIntentResponseDto?> CreatePaymentIntentAsync(int orderId);

    Task<RefundResponseDto?> RefundAsync(int orderId, decimal? amount, string? reason);

    Task<List<PaymentDto>> GetHistoryAsync();

    Task<AdminPaymentHistoryResultDto> GetAllForAdminAsync(AdminPaymentQuery query);
}
