using System.Security.Claims;
using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/refund-requests")]
    [Authorize]
    public class RefundRequestsController : ControllerBase
    {
        private readonly IRefundRequestService _refundRequestService;

        public RefundRequestsController(IRefundRequestService refundRequestService)
        {
            _refundRequestService = refundRequestService;
        }

        // TODO: same claim-type note as PaymentsController -- confirm this matches your AuthService.
        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        [HttpPost]
        public async Task<ActionResult<RefundRequestDto>> Create([FromBody] CreateRefundRequestDto dto)
        {
            var result = await _refundRequestService.CreateAsync(dto.OrderId, CurrentUserId, dto.Reason);
            return Ok(result);
        }

        [HttpGet("mine")]
        public async Task<ActionResult<List<RefundRequestDto>>> Mine()
        {
            var result = await _refundRequestService.GetMineAsync(CurrentUserId);
            return Ok(result);
        }
    }
}
