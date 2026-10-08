using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services;

public class AdminOrderService : IAdminOrderService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<AdminOrderService> _logger;

    public AdminOrderService(
        AppDbContext context,
        IEmailService emailService,
        ILogger<AdminOrderService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<PagedResultDto<AdminOrderDto>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = _context.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new AdminOrderDto
            {
                Id = o.Id,
                CustomerName = o.User.FullName,
                ItemCount = o.OrderItems.Count,
                TotalPrice = o.TotalAmount,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                CreatedAt = o.OrderDate
            })
            .ToListAsync();

        return new PagedResultDto<AdminOrderDto>
        {
            Items = items,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AdminOrderDetailsDto?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(order => new AdminOrderDetailsDto
            {
                Id = order.Id,
                CustomerName = order.FullName,
                PhoneNumber = order.PhoneNumber,
                Address = order.Address,
                City = order.City,
                PostalCode = order.PostalCode,
                Country = order.Country,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                PaymentMethod = order.PaymentMethod,
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,

                Items = order.OrderItems
                    .Select(item => new AdminOrderItemDto
                    {
                        SKU = item.SKU,
                        ProductName = item.Product.Name,
                        Price = item.UnitPrice,
                        Quantity = item.Quantity
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task UpdateStatusAsync(
        int id,
        AdminUpdateOrderStatusDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
            throw new BadRequestException("Order status is required.");

        var newStatus = dto.Status.Trim();

        var validStatuses = new[]
        {
            "Pending",
            "Processing",
            "Shipped",
            "Delivered",
            "Cancelled",
            "Refunded"
        };

        var validStatus = validStatuses.FirstOrDefault(
            s => string.Equals(s, newStatus, StringComparison.OrdinalIgnoreCase));

        if (validStatus == null)
            throw new BadRequestException(
                $"Invalid order status. Valid statuses are: {string.Join(", ", validStatuses)}.");

        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            throw new NotFoundException("Order not found.");

        var oldStatus = order.Status;

        if (string.Equals(oldStatus, validStatus, StringComparison.OrdinalIgnoreCase))
            return;

        order.Status = validStatus;

        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync(
        action: "OrderStatusUpdated",
        entityName: "Order",
        entityId: order.Id.ToString(),
        details: $"Order #{order.Id} status changed from {oldStatus} to {validStatus}.");

        try
        {
            if (order.User != null &&
                !string.IsNullOrWhiteSpace(order.User.Email))
            {
                var subject = GetOrderStatusEmailSubject(
                    order.Id,
                    validStatus);

                var htmlBody = BuildOrderStatusEmail(
                    order.User.FullName,
                    order.Id,
                    oldStatus,
                    validStatus,
                    order.TotalAmount);

                await _emailService.SendAsync(
                    order.User.Email,
                    subject,
                    htmlBody);

                _logger.LogInformation(
                    "Order status email sent successfully. OrderId: {OrderId}, CustomerEmail: {Email}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                    order.Id,
                    order.User.Email,
                    oldStatus,
                    validStatus);
            }
            else
            {
                _logger.LogWarning(
                    "Order status was updated but customer email is missing. OrderId: {OrderId}",
                    order.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Order status was updated but status email failed. OrderId: {OrderId}, OldStatus: {OldStatus}, NewStatus: {NewStatus}",
                order.Id,
                oldStatus,
                validStatus);
        }
    }

    private readonly IAuditLogService _auditLogService;

    public AdminOrderService(
        AppDbContext context,
        IEmailService emailService,
        IAuditLogService auditLogService,
        ILogger<AdminOrderService> logger)
    {
        _context = context;
        _emailService = emailService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    private static string GetOrderStatusEmailSubject(
        int orderId,
        string status)
    {
        return status switch
        {
            "Pending" =>
                $"Order #{orderId} received",

            "Processing" =>
                $"Order #{orderId} is being processed",

            "Shipped" =>
                $"Order #{orderId} has been shipped",

            "Delivered" =>
                $"Order #{orderId} has been delivered",

            "Cancelled" =>
                $"Order #{orderId} has been cancelled",

            "Refunded" =>
                $"Order #{orderId} has been refunded",

            _ =>
                $"Order #{orderId} status updated"
        };
    }

    private static string BuildOrderStatusEmail(
        string? customerName,
        int orderId,
        string oldStatus,
        string newStatus,
        decimal totalAmount)
    {
        var safeName = System.Net.WebUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(customerName)
                ? "Customer"
                : customerName);

        var safeOldStatus = System.Net.WebUtility.HtmlEncode(oldStatus);
        var safeNewStatus = System.Net.WebUtility.HtmlEncode(newStatus);

        var formattedTotal = totalAmount.ToString("C");

        var message = newStatus switch
        {
            "Pending" =>
                "We have received your order and it is currently waiting to be processed.",

            "Processing" =>
                "Your order is now being prepared. We will notify you when it has been shipped.",

            "Shipped" =>
                "Good news! Your order has been shipped and is on its way to you.",

            "Delivered" =>
                "Your order has been delivered. We hope you enjoy your purchase!",

            "Cancelled" =>
                "Your order has been cancelled. If you did not request this cancellation, please contact our support team.",

            "Refunded" =>
                "Your order has been refunded. Please allow your payment provider some time to process the refund.",

            _ =>
                "The status of your order has been updated."
        };

        var safeMessage = System.Net.WebUtility.HtmlEncode(message);

        return $"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8">
            <meta name="viewport" content="width=device-width, initial-scale=1.0">
            <title>Order Status Update</title>
        </head>

        <body style="margin:0; padding:0; background-color:#f5f5f5; font-family:Arial,Helvetica,sans-serif;">

            <div style="max-width:600px; margin:40px auto; background:#ffffff; border-radius:8px; overflow:hidden;">

                <div style="padding:24px; text-align:center; background:#111827; color:#ffffff;">
                    <h1 style="margin:0; font-size:24px;">
                        ECommerce
                    </h1>
                </div>

                <div style="padding:32px;">

                    <h2 style="margin-top:0;">
                        Order #{orderId}
                    </h2>

                    <p>
                        Hello {safeName},
                    </p>

                    <p>
                        {safeMessage}
                    </p>

                    <div style="margin:25px 0; padding:20px; background:#f9fafb; border-radius:6px;">

                        <p style="margin:0 0 10px 0;">
                            <strong>Previous status:</strong>
                            {safeOldStatus}
                        </p>

                        <p style="margin:0 0 10px 0;">
                            <strong>Current status:</strong>
                            {safeNewStatus}
                        </p>

                        <p style="margin:0;">
                            <strong>Order total:</strong>
                            {formattedTotal}
                        </p>

                    </div>

                    <p>
                        Thank you for shopping with us.
                    </p>

                </div>

                <div style="padding:20px; text-align:center; background:#f9fafb; color:#6b7280; font-size:13px;">
                    This is an automated email from ECommerce.
                </div>

            </div>

        </body>
        </html>
        """;
    }
}