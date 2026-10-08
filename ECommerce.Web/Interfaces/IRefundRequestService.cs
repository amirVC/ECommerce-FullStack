using ECommerce.Web.DTOs;
namespace ECommerce.Web.Interfaces;
public interface IRefundRequestService
{
    Task<RefundRequestDto?> CreateAsync(int orderId, string reason);
    Task<List<RefundRequestDto>> GetMineAsync();

    Task<List<AdminRefundRequestDto>> GetAllAsync();
    Task<AdminRefundRequestDto?> ApproveAsync(int id, string? adminNote);
    Task<AdminRefundRequestDto?> RejectAsync(int id, string? adminNote);
}