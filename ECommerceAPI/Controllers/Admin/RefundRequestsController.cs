using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/refund-requests")]
    [Authorize(Roles = "Admin")]
    public class RefundRequestsController : ControllerBase
    {
        private readonly IRefundRequestService _refundRequestService;

        public RefundRequestsController(IRefundRequestService refundRequestService)
        {
            _refundRequestService = refundRequestService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<AdminRefundRequestDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _refundRequestService.GetAllAsync(page, pageSize);
            return Ok(result);
        }

        [HttpPost("{id}/approve")]
        public async Task<ActionResult<AdminRefundRequestDto>> Approve(int id, [FromBody] ReviewRefundRequestDto dto)
        {
            var result = await _refundRequestService.ApproveAsync(id, dto.AdminNote);
            return Ok(result);
        }

        [HttpPost("{id}/reject")]
        public async Task<ActionResult<AdminRefundRequestDto>> Reject(int id, [FromBody] ReviewRefundRequestDto dto)
        {
            var result = await _refundRequestService.RejectAsync(id, dto.AdminNote);
            return Ok(result);
        }
    }
}