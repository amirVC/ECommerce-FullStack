using ECommerceAPI.DTOs;
using ECommerceAPI.Services;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var message = await _authService.Register(dto);

            if (message == null)
                return BadRequest("Email already exists.");

            return Ok(new { message });
        }

        // Step 0: password check. Every successful login lands here and
        // moves on to the email OTP step — there is no direct-success path.
        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.Login(dto, GetIpAddress());

            if (result.AccountLocked)
            {
                return StatusCode(StatusCodes.Status423Locked, new
                {
                    message = $"Too many failed attempts. Account is locked until {result.LockoutEnd:u} UTC."
                });
            }

            if (result.RequiresEmailOtp)
            {
                return Ok(new AuthResponseDto
                {
                    RequiresEmailOtp = true,
                    ChallengeToken = result.ChallengeToken,
                    MaskedEmail = result.MaskedEmail
                });
            }

            if (!result.Success)
                return Unauthorized("Invalid email or password.");

            return Ok(new AuthResponseDto
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            });
        }

        // Step 1: confirm the code emailed after step 0. Every user goes through this.
        [HttpPost("login/verify-email-otp")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> VerifyEmailOtp(ChallengeCodeDto dto)
        {
            var result = await _authService.VerifyLoginEmailOtp(dto, GetIpAddress());

            if (result.RequiresTotp)
            {
                return Ok(new AuthResponseDto
                {
                    RequiresTotp = true,
                    ChallengeToken = result.ChallengeToken
                });
            }

            if (!result.Success)
                return Unauthorized("Invalid or expired code.");

            return Ok(new AuthResponseDto
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            });
        }

        // Step 2, only reached if the account has TOTP enabled.
        [HttpPost("login/verify-totp")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> VerifyTotp(ChallengeCodeDto dto)
        {
            var result = await _authService.VerifyLoginTotp(dto, GetIpAddress());

            if (!result.Success)
                return Unauthorized("Invalid or expired code.");

            return Ok(new AuthResponseDto
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            });
        }

        [HttpPost("refresh")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Refresh(RefreshTokenRequestDto dto)
        {
            var result = await _authService.RefreshToken(dto.RefreshToken, GetIpAddress());

            if (!result.Success)
                return Unauthorized("Invalid or expired refresh token.");

            return Ok(new AuthResponseDto
            {
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            });
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(RefreshTokenRequestDto dto)
        {
            var success = await _authService.RevokeRefreshToken(dto.RefreshToken, GetIpAddress());

            if (!success)
                return BadRequest(new { message = "Token not found or already revoked." });

            return Ok(new { message = "Token revoked." });
        }

        // ---- Email confirmation / password reset (unchanged) ----

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            var success = await _authService.ConfirmEmail(token);

            if (!success)
                return BadRequest(new { message = "Invalid or expired confirmation link." });

            return Ok(new { message = "Email confirmed successfully." });
        }

        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail([FromServices] IEmailService emailService)
        {
            await emailService.SendAsync(
                "ecommerce01.proj@gmail.com",
                "ECommerce SMTP Test",
                """
        <h1>SMTP works!</h1>
        <p>This email was sent using Gmail SMTP.</p>
        """);

            return Ok("Email sent.");
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            await _authService.ForgotPassword(dto.Email);

            // Always return the same response to prevent email/account enumeration.
            return Ok(new
            {
                message =
                    "If an account with that email exists, " +
                    "a password reset link has been sent."
            });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var success = await _authService.ResetPassword(dto);

            if (!success)
            {
                return BadRequest(new
                {
                    message = "The password reset link is invalid or expired."
                });
            }

            return Ok(new { message = "Password has been reset successfully." });
        }

        // ---- Helpers ----

        private string? GetIpAddress() =>
            HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}