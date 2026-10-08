using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Models;
using ECommerceAPI.Options;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace ECommerceAPI.Services
{
    public class StripePaymentService : IPaymentService
    {
        private readonly AppDbContext _db;
        private readonly StripeOptions _options;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(
            AppDbContext db,
            IOptions<StripeOptions> options,
            ILogger<StripePaymentService> logger)
        {
            _db = db;
            _options = options.Value;
            _logger = logger;

            StripeConfiguration.ApiKey = _options.SecretKey;
        }

        public async Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(int orderId, int userId)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                throw new NotFoundException($"Order {orderId} not found.");

            if (order.UserId != userId)
                throw new UnauthorizedException("This order does not belong to the current user.");

            var existing = await _db.Payments
                .Where(p => p.OrderId == orderId && p.Status == PaymentStatus.Pending)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            var amountInCents = (long)Math.Round(order.TotalAmount * 100, MidpointRounding.AwayFromZero);

            var service = new PaymentIntentService();
            PaymentIntent intent;

            if (existing != null && !string.IsNullOrEmpty(existing.PaymentIntentId))
            {
                intent = await service.UpdateAsync(existing.PaymentIntentId, new PaymentIntentUpdateOptions
                {
                    Amount = amountInCents
                });
            }
            else
            {
                intent = await service.CreateAsync(new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = "usd",
                    Metadata = new Dictionary<string, string>
                    {
                        { "orderId", order.Id.ToString() },
                        { "userId", userId.ToString() }
                    },
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    }
                });

                var payment = new Payment
                {
                    OrderId = order.Id,
                    Provider = PaymentProvider.Stripe,
                    PaymentIntentId = intent.Id,
                    Amount = order.TotalAmount,
                    Currency = "usd",
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Payments.Add(payment);
                await _db.SaveChangesAsync();
            }

            return new PaymentIntentResponseDto
            {
                ClientSecret = intent.ClientSecret,
                PaymentIntentId = intent.Id,
                Amount = order.TotalAmount,
                Currency = "usd"
            };
        }

        public async Task<bool> HandleWebhookAsync(string json, string stripeSignatureHeader)
        {
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignatureHeader,
                    _options.WebhookSecret);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature verification failed.");
                throw new BadRequestException("Invalid Stripe webhook signature.");
            }

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    {
                        var intent = (PaymentIntent)stripeEvent.Data.Object;
                        await UpdatePaymentStatusAsync(intent.Id, PaymentStatus.Succeeded, null);
                        return true;
                    }
                case "payment_intent.payment_failed":
                    {
                        var intent = (PaymentIntent)stripeEvent.Data.Object;
                        var reason = intent.LastPaymentError?.Message ?? "Payment failed.";
                        await UpdatePaymentStatusAsync(intent.Id, PaymentStatus.Failed, reason);
                        return true;
                    }
                case "payment_intent.canceled":
                    {
                        var intent = (PaymentIntent)stripeEvent.Data.Object;
                        await UpdatePaymentStatusAsync(intent.Id, PaymentStatus.Canceled, null);
                        return true;
                    }
                default:
                    _logger.LogInformation("Ignored Stripe event type: {Type}", stripeEvent.Type);
                    return false;
            }
        }

        private async Task UpdatePaymentStatusAsync(string paymentIntentId, PaymentStatus status, string? failureReason)
        {
            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId);

            if (payment == null)
            {
                _logger.LogWarning("Received webhook for unknown PaymentIntent {Id}", paymentIntentId);
                return;
            }

            payment.Status = status;
            payment.FailureReason = failureReason;
            payment.UpdatedAt = DateTime.UtcNow;

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == payment.OrderId);
            if (order != null)
            {
                order.PaymentStatus = status switch
                {
                    PaymentStatus.Succeeded => "Paid",
                    _ => status.ToString()
                };

                switch (status)
                {
                    case PaymentStatus.Succeeded:
                        order.PaymentMethod = "Stripe";
                        if (string.Equals(order.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                        {
                            order.Status = "Processing";
                        }
                        break;

                    case PaymentStatus.Failed:
                    case PaymentStatus.Canceled:
                        break;
                }
            }

            await _db.SaveChangesAsync();
        }
        public async Task<RefundResponseDto> RefundAsync(int orderId, decimal? amount, string? reason)
        {
            var payment = await _db.Payments
                .Where(p => p.OrderId == orderId && p.Status == PaymentStatus.Succeeded)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            if (payment == null)
                throw new NotFoundException($"No successful payment found for order {orderId}.");

            if (string.IsNullOrEmpty(payment.PaymentIntentId))
                throw new BadRequestException("Payment has no associated Stripe PaymentIntent.");

            var remaining = payment.Amount - payment.RefundedAmount;
            var refundAmount = amount ?? remaining;

            if (refundAmount <= 0 || refundAmount > remaining)
                throw new BadRequestException($"Refund amount must be between 0 and {remaining}.");

            var refundService = new RefundService();
            var refund = await refundService.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = payment.PaymentIntentId,
                Amount = (long)Math.Round(refundAmount * 100, MidpointRounding.AwayFromZero),
                Reason = "requested_by_customer"
            });

            payment.RefundedAmount += refundAmount;
            payment.Status = payment.RefundedAmount >= payment.Amount
                ? PaymentStatus.Refunded
                : PaymentStatus.PartiallyRefunded;
            payment.UpdatedAt = DateTime.UtcNow;

            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order != null)
            {
                order.PaymentStatus = payment.Status.ToString(); // "Refunded" or "PartiallyRefunded"
            }
            await _db.SaveChangesAsync();

            return new RefundResponseDto
            {
                PaymentId = payment.Id,
                RefundedAmount = payment.RefundedAmount,
                Status = payment.Status
            };
        }

        public async Task<List<PaymentDto>> GetHistoryForUserAsync(int userId)
        {
            return await _db.Payments
                .AsNoTracking()
                .Where(p => p.Order != null && p.Order.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => ToDto(p))
                .ToListAsync();
        }

        public async Task<List<PaymentDto>> GetHistoryForOrderAsync(int orderId)
        {
            return await _db.Payments
                .AsNoTracking()
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => ToDto(p))
                .ToListAsync();
        }

        private static PaymentDto ToDto(Payment p) => new PaymentDto
        {
            Id = p.Id,
            OrderId = p.OrderId,
            Provider = p.Provider,
            Amount = p.Amount,
            Currency = p.Currency,
            Status = p.Status,
            RefundedAmount = p.RefundedAmount,
            CreatedAt = p.CreatedAt
        };
    }
}