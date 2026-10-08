using System.Security.Claims;
using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        private int CurrentUserId =>
            int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "0");

        [HttpPost("create-intent")]
        [Authorize]
        [EnableRateLimiting("payment")]
        public async Task<ActionResult<PaymentIntentResponseDto>> CreateIntent(
            [FromBody] CreatePaymentIntentDto dto)
        {
            var result =
                await _paymentService.CreatePaymentIntentAsync(
                    dto.OrderId,
                    CurrentUserId);

            return Ok(result);
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);

            var json = await reader.ReadToEndAsync();

            var signature = Request.Headers["Stripe-Signature"];

            await _paymentService.HandleWebhookAsync(
                json,
                signature!);

            return Ok();
        }

        [HttpPost("{orderId}/refund")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("payment")]
        public async Task<ActionResult<RefundResponseDto>> Refund(
            int orderId,
            [FromBody] DirectRefundDto dto)
        {
            var result =
                await _paymentService.RefundAsync(
                    orderId,
                    dto.Amount,
                    dto.Reason);

            return Ok(result);
        }

        [HttpGet("history")]
        [Authorize]
        public async Task<ActionResult<List<PaymentDto>>> History()
        {
            var result =
                await _paymentService.GetHistoryForUserAsync(
                    CurrentUserId);

            return Ok(result);
        }

        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<ActionResult<List<PaymentDto>>> HistoryForOrder(
            int orderId)
        {
            var result =
                await _paymentService.GetHistoryForOrderAsync(
                    orderId);

            return Ok(result);
        }
    }
}