using System.Linq.Expressions;
using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.QueryParameters;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services
{
    public class AdminPaymentService : IAdminPaymentService
    {
        private readonly AppDbContext _db;

        public AdminPaymentService(AppDbContext db)
        {
            _db = db;
        }

        private static readonly Expression<Func<Payment, AdminPaymentDto>> ProjectToDto = p => new AdminPaymentDto
        {
            Id = p.Id,
            OrderId = p.OrderId,
            UserId = p.Order != null ? p.Order.UserId : 0,
            CustomerName = p.Order != null ? p.Order.FullName : string.Empty,
            CustomerEmail = p.Order != null && p.Order.User != null ? p.Order.User.Email : string.Empty,
            Provider = p.Provider,
            PaymentIntentId = p.PaymentIntentId,
            Amount = p.Amount,
            Currency = p.Currency,
            Status = p.Status,
            RefundedAmount = p.RefundedAmount,
            FailureReason = p.FailureReason,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };

        public async Task<AdminPaymentHistoryResultDto> GetAllAsync(PaymentQueryParameters query)
        {
            var payments = _db.Payments
                .AsNoTracking()
                .AsQueryable();

            if (query.Status.HasValue)
                payments = payments.Where(p => p.Status == query.Status.Value);

            if (query.Provider.HasValue)
                payments = payments.Where(p => p.Provider == query.Provider.Value);

            if (query.OrderId.HasValue)
                payments = payments.Where(p => p.OrderId == query.OrderId.Value);

            if (query.UserId.HasValue)
                payments = payments.Where(p => p.Order != null && p.Order.UserId == query.UserId.Value);

            if (query.FromDate.HasValue)
                payments = payments.Where(p => p.CreatedAt >= query.FromDate.Value.Date);

            if (query.ToDate.HasValue)
                payments = payments.Where(p => p.CreatedAt < query.ToDate.Value.Date.AddDays(1));

            payments = payments.OrderByDescending(p => p.CreatedAt);

            var totalCount = await payments.CountAsync();

            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

            var items = await payments
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ProjectToDto)
                .ToListAsync();

            return new AdminPaymentHistoryResultDto
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<AdminPaymentDto?> GetByIdAsync(int id)
        {
            return await _db.Payments
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(ProjectToDto)
                .FirstOrDefaultAsync();
        }
    }
}