using ECommerceAPI.DTOs;
using ECommerceAPI.QueryParameters;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IAdminPaymentService
    {
        // Paged, filterable list of ALL payments across all users/orders.
        Task<AdminPaymentHistoryResultDto> GetAllAsync(PaymentQueryParameters query);

        Task<AdminPaymentDto?> GetByIdAsync(int id);
    }
}
