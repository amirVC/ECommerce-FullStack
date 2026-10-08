using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReviewsController : ControllerBase
    {
        private readonly IAdminReviewService _adminReviewService;
        public ReviewsController(IAdminReviewService adminReviewService) => _adminReviewService = adminReviewService;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
            => Ok(await _adminReviewService.GetAllReviewsAsync(page, pageSize, status));

        [HttpPost("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] AdminUpdateReviewStatusDto dto)
            => Ok(await _adminReviewService.UpdateStatusAsync(id, dto.Status));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _adminReviewService.DeleteReviewAsync(id);
            return NoContent();
        }
    }
}