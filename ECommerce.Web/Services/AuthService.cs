using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ECommerce.Web.DTOs;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ECommerce.Web.Services;

public class AuthService : IAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApiClient _apiClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthService(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IApiClient apiClient)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _apiClient = apiClient;
    }

    public async Task<string?> RegisterAsync(RegisterDto dto)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        var content = JsonContent(dto);

        var response = await client.PostAsync("api/auth/register", content);

        if (!response.IsSuccessStatusCode)
            return null;

        var responseJson = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseJson))
            return "Registration successful. Please check your email to confirm your account.";

        try
        {
            var result = JsonSerializer.Deserialize<RegisterResponseDto>(responseJson, JsonOptions);

            return result?.Message
                   ?? "Registration successful. Please check your email to confirm your account.";
        }
        catch
        {
            return "Registration successful. Please check your email to confirm your account.";
        }
    }

    public async Task<LoginResult> LoginAsync(LoginDto dto)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        var response = await client.PostAsync("api/auth/login", JsonContent(dto));

        if (response.StatusCode == HttpStatusCode.Locked)
        {
            var message = await ExtractMessageAsync(response)
                           ?? "Too many failed attempts. Your account is temporarily locked.";
            return LoginResult.Locked(message);
        }

        if (!response.IsSuccessStatusCode)
            return LoginResult.Invalid();

        var responseJson = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseJson, JsonOptions);

        return await ResolveLoginResponseAsync(loginResponse);
    }

    // Step 1: confirm the code emailed after password check.
    public async Task<LoginResult> VerifyEmailOtpAsync(string challengeToken, string code)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        var dto = new VerifyCodeRequestDto
        {
            ChallengeToken = challengeToken,
            Code = code
        };

        var response = await client.PostAsync("api/auth/login/verify-email-otp", JsonContent(dto));

        if (!response.IsSuccessStatusCode)
            return LoginResult.Invalid();

        var responseJson = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseJson, JsonOptions);

        return await ResolveLoginResponseAsync(loginResponse);
    }

    // Step 2, only reached if the account has TOTP enabled.
    public async Task<LoginResult> VerifyTotpAsync(string challengeToken, string code)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        var dto = new VerifyCodeRequestDto
        {
            ChallengeToken = challengeToken,
            Code = code
        };

        var response = await client.PostAsync("api/auth/login/verify-totp", JsonContent(dto));

        if (!response.IsSuccessStatusCode)
            return LoginResult.Invalid();

        var responseJson = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseJson, JsonOptions);

        return await ResolveLoginResponseAsync(loginResponse);
    }

    public async Task<bool> ConfirmEmailAsync(string token)
    {
        try
        {
            var encodedToken = Uri.EscapeDataString(token);

            var result = await _apiClient.GetAsync<ConfirmEmailResponse>(
                $"api/auth/confirm-email?token={encodedToken}");

            return result != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext!;

        var refreshToken = httpContext.Session.GetString("RefreshToken");

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                await client.PostAsync(
                    "api/auth/revoke",
                    JsonContent(new RefreshTokenRequestDto { RefreshToken = refreshToken }));
            }
            catch
            {
                // Best-effort server-side revoke; local sign-out proceeds regardless.
            }
        }

        httpContext.Session.Remove("JWToken");
        httpContext.Session.Remove("RefreshToken");

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public bool IsLoggedIn() => !string.IsNullOrWhiteSpace(GetToken());

    public string? GetToken() =>
        _httpContextAccessor.HttpContext?.Session.GetString("JWToken");

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        var response = await client.PostAsync(
            "api/auth/forgot-password",
            JsonContent(new { Email = email }));

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResetPasswordAsync(
        string token,
        string newPassword,
        string confirmPassword)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        var dto = new
        {
            Token = token,
            NewPassword = newPassword,
            ConfirmPassword = confirmPassword
        };

        var response = await client.PostAsync("api/auth/reset-password", JsonContent(dto));

        return response.IsSuccessStatusCode;
    }

    // ================================================================
    // Security: TOTP two-factor management
    // ================================================================
    // These go through _apiClient rather than a raw HttpClient because
    // they're authorized calls — _apiClient attaches the bearer token
    // from the current session automatically.

    public async Task<bool> IsTotpEnabledAsync()
    {
        try
        {
            var result = await _apiClient.GetAsync<TotpStatusResponseDto>("api/security/2fa/status");
            return result?.Enabled ?? false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<TotpSetupResponseDto?> SetupTotpAsync()
    {
        try
        {
            return await _apiClient.PostAsync<object, TotpSetupResponseDto>(
                "api/security/2fa/setup",
                new { });
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> EnableTotpAsync(string code)
    {
        try
        {
            return await _apiClient.PostAsync(
                "api/security/2fa/enable",
                new { Code = code });
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DisableTotpAsync(string code)
    {
        try
        {
            return await _apiClient.PostAsync(
                "api/security/2fa/disable",
                new { Code = code });
        }
        catch
        {
            return false;
        }
    }

    // ================================================================
    // Helpers
    // ================================================================

    private async Task<LoginResult> ResolveLoginResponseAsync(LoginResponseDto? loginResponse)
    {
        if (loginResponse == null)
            return LoginResult.Invalid();

        if (loginResponse.RequiresEmailOtp)
        {
            if (string.IsNullOrWhiteSpace(loginResponse.ChallengeToken))
                return LoginResult.Invalid();

            return LoginResult.EmailOtpRequired(loginResponse.ChallengeToken, loginResponse.MaskedEmail);
        }

        if (loginResponse.RequiresTotp)
        {
            if (string.IsNullOrWhiteSpace(loginResponse.ChallengeToken))
                return LoginResult.Invalid();

            return LoginResult.TotpRequired(loginResponse.ChallengeToken);
        }

        if (string.IsNullOrWhiteSpace(loginResponse.AccessToken) ||
            string.IsNullOrWhiteSpace(loginResponse.RefreshToken))
        {
            return LoginResult.Invalid();
        }

        await CompleteSignInAsync(loginResponse.AccessToken, loginResponse.RefreshToken);
        return LoginResult.Success();
    }

    private async Task CompleteSignInAsync(string accessToken, string refreshToken)
    {
        var httpContext = _httpContextAccessor.HttpContext!;

        httpContext.Session.SetString("JWToken", accessToken);
        httpContext.Session.SetString("RefreshToken", refreshToken);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);

        var identity = new ClaimsIdentity(
            jwt.Claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        var principal = new ClaimsPrincipal(identity);

        // The access token itself only lives ~15 minutes and gets silently
        // refreshed in the background (see TokenRefreshHandler), so the
        // sign-in cookie's lifetime is intentionally decoupled from
        // jwt.ValidTo — otherwise the user would be kicked back to the
        // login page every 15 minutes even with a valid refresh token.
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            });
    }

    private static async Task<string?> ExtractMessageAsync(HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
                return null;

            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("message", out var prop))
                return prop.GetString();

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static StringContent JsonContent(object dto) =>
        new(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

    private class RegisterResponseDto
    {
        public string Message { get; set; } = string.Empty;
    }

    private class ConfirmEmailResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}