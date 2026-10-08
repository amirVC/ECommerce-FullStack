using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services.Interfaces
{
    public interface IRefundRequestService
    {
        Task<RefundRequestDto> CreateAsync(int orderId, int userId, string reason);
        Task<List<RefundRequestDto>> GetMineAsync(int userId);

        Task<PagedResultDto<AdminRefundRequestDto>> GetAllAsync(int page = 1, int pageSize = 20);
        Task<AdminRefundRequestDto> ApproveAsync(int requestId, string? adminNote);
        Task<AdminRefundRequestDto> RejectAsync(int requestId, string? adminNote);
    }
}