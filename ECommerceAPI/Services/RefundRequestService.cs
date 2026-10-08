using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services
{
    public class RefundRequestService : IRefundRequestService
    {
        private readonly AppDbContext _db;
        private readonly IPaymentService _paymentService;

        public RefundRequestService(AppDbContext db, IPaymentService paymentService)
        {
            _db = db;
            _paymentService = paymentService;
        }

        public async Task<RefundRequestDto> CreateAsync(int orderId, int userId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new BadRequestException("A reason is required to request a refund.");

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                throw new NotFoundException($"Order {orderId} not found.");

            if (order.UserId != userId)
                throw new UnauthorizedException("This order does not belong to the current user.");

            if (!string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Only paid orders can have a refund requested.");

            var alreadyPending = await _db.RefundRequests.AnyAsync(r =>
                r.OrderId == orderId && r.Status == RefundRequestStatus.Pending);
            if (alreadyPending)
                throw new BadRequestException("A refund request for this order is already pending review.");

            var request = new RefundRequest
            {
                OrderId = orderId,
                UserId = userId,
                Reason = reason.Trim(),
                Status = RefundRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _db.RefundRequests.Add(request);
            await _db.SaveChangesAsync();

            return ToDto(request);
        }

        public async Task<List<RefundRequestDto>> GetMineAsync(int userId)
        {
            return await _db.RefundRequests
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new RefundRequestDto
                {
                    Id = r.Id,
                    OrderId = r.OrderId,
                    Reason = r.Reason,
                    Status = r.Status,
                    RequestedAt = r.RequestedAt,
                    ReviewedAt = r.ReviewedAt,
                    AdminNote = r.AdminNote
                })
                .ToListAsync();
        }

        public async Task<PagedResultDto<AdminRefundRequestDto>> GetAllAsync(int page = 1, int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 20 : pageSize;

            var query = _db.RefundRequests
                .AsNoTracking()
                .OrderByDescending(r => r.RequestedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new AdminRefundRequestDto
                {
                    Id = r.Id,
                    OrderId = r.OrderId,
                    CustomerName = r.User != null ? r.User.FullName : string.Empty,
                    OrderTotal = r.Order != null ? r.Order.TotalAmount : 0,
                    Reason = r.Reason,
                    Status = r.Status,
                    RequestedAt = r.RequestedAt,
                    ReviewedAt = r.ReviewedAt,
                    AdminNote = r.AdminNote
                })
                .ToListAsync();

            return new PagedResultDto<AdminRefundRequestDto>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<AdminRefundRequestDto> ApproveAsync(int requestId, string? adminNote)
        {
            var request = await _db.RefundRequests
                .Include(r => r.Order)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                throw new NotFoundException($"Refund request {requestId} not found.");

            if (request.Status != RefundRequestStatus.Pending)
                throw new BadRequestException("Only pending refund requests can be approved.");

            await _paymentService.RefundAsync(request.OrderId, amount: null, reason: request.Reason);

            request.Status = RefundRequestStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow;
            request.AdminNote = adminNote;

            await _db.SaveChangesAsync();

            return new AdminRefundRequestDto
            {
                Id = request.Id,
                OrderId = request.OrderId,
                CustomerName = request.User?.FullName ?? string.Empty,
                OrderTotal = request.Order?.TotalAmount ?? 0,
                Reason = request.Reason,
                Status = request.Status,
                RequestedAt = request.RequestedAt,
                ReviewedAt = request.ReviewedAt,
                AdminNote = request.AdminNote
            };
        }

        public async Task<AdminRefundRequestDto> RejectAsync(int requestId, string? adminNote)
        {
            var request = await _db.RefundRequests
                .Include(r => r.Order)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                throw new NotFoundException($"Refund request {requestId} not found.");

            if (request.Status != RefundRequestStatus.Pending)
                throw new BadRequestException("Only pending refund requests can be rejected.");

            request.Status = RefundRequestStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow;
            request.AdminNote = adminNote;

            await _db.SaveChangesAsync();

            return new AdminRefundRequestDto
            {
                Id = request.Id,
                OrderId = request.OrderId,
                CustomerName = request.User?.FullName ?? string.Empty,
                OrderTotal = request.Order?.TotalAmount ?? 0,
                Reason = request.Reason,
                Status = request.Status,
                RequestedAt = request.RequestedAt,
                ReviewedAt = request.ReviewedAt,
                AdminNote = request.AdminNote
            };
        }

        private static RefundRequestDto ToDto(RefundRequest r) => new RefundRequestDto
        {
            Id = r.Id,
            OrderId = r.OrderId,
            Reason = r.Reason,
            Status = r.Status,
            RequestedAt = r.RequestedAt,
            ReviewedAt = r.ReviewedAt,
            AdminNote = r.AdminNote
        };
    }
}