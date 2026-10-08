using ECommerceAPI.DTOs;
using ECommerceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ECommerceAPI.Controllers
{
    // Account security settings for the logged-in user.
    // TOTP is optional on top of the mandatory email OTP login flow.
    [ApiController]
    [Route("api/security")]
    [Authorize]
    public class SecurityController : ControllerBase
    {
        private readonly AuthService _authService;

        public SecurityController(AuthService authService)
        {
            _authService = authService;
        }

        // ==========================================
        // Get 2FA status
        // Global rate limit: 100 requests / minute
        // ==========================================
        [HttpGet("2fa/status")]
        public async Task<IActionResult> GetTwoFactorStatus()
        {
            var enabled = await _authService.IsTotpEnabled(GetUserId());

            return Ok(new TotpStatusResponseDto
            {
                Enabled = enabled
            });
        }

        // ==========================================
        // Setup TOTP
        // 5 requests / 5 minutes
        // ==========================================
        //
        // Generates a secret and QR data.
        //
        [HttpPost("2fa/setup")]
        [EnableRateLimiting("otp")]
        public async Task<IActionResult> SetupTotp()
        {
            var result = await _authService.SetupTotp(GetUserId());

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // ==========================================
        // Enable TOTP
        // 5 requests / 5 minutes
        // ==========================================
        //
        // Confirms the authenticator-app code.
        //
        [HttpPost("2fa/enable")]
        [EnableRateLimiting("otp")]
        public async Task<IActionResult> EnableTotp(
            TwoFactorVerifyDto dto)
        {
            var success = await _authService.EnableTotp(
                GetUserId(),
                dto.Code);

            if (!success)
            {
                return BadRequest(new
                {
                    message = "Invalid or expired code."
                });
            }

            return Ok(new
            {
                message = "Two-factor authentication enabled."
            });
        }

        // ==========================================
        // Disable TOTP
        // 5 requests / 5 minutes
        // ==========================================
        //
        // Requires the current authenticator code.
        //
        [HttpPost("2fa/disable")]
        [EnableRateLimiting("otp")]
        public async Task<IActionResult> DisableTotp(
            Disable2FaDto dto)
        {
            var success = await _authService.DisableTotp(
                GetUserId(),
                dto.Code);

            if (!success)
            {
                return BadRequest(new
                {
                    message = "Invalid code."
                });
            }

            return Ok(new
            {
                message = "Two-factor authentication disabled."
            });
        }

        // ==========================================
        // Current user ID
        // ==========================================
        private int GetUserId() =>
            int.Parse(
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);
    }
}