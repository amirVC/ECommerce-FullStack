using ECommerce.Web.DTOs;

namespace ECommerce.Web.Interfaces;

public interface IAuthService
{
    Task<string?> RegisterAsync(RegisterDto dto);
    Task<Services.LoginResult> LoginAsync(LoginDto dto);
    Task<Services.LoginResult> VerifyEmailOtpAsync(string challengeToken, string code);
    Task<Services.LoginResult> VerifyTotpAsync(string challengeToken, string code);
    Task LogoutAsync();
    bool IsLoggedIn();
    string? GetToken();
    Task<bool> ConfirmEmailAsync(string token);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(
        string token,
        string newPassword,
        string confirmPassword);

    // ---- Security: TOTP two-factor management ----
    Task<bool> IsTotpEnabledAsync();
    Task<TotpSetupResponseDto?> SetupTotpAsync();
    Task<bool> EnableTotpAsync(string code);
    Task<bool> DisableTotpAsync(string code);
}