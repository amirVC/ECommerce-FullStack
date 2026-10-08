using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OtpNet;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommerceAPI.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;
        private readonly IAuditLogService _auditLogService;


        // ---- Login email OTP tuning ----
        private const int LoginOtpExpiryMinutes = 10;
        private const int LoginOtpMaxAttempts = 5;
        private const int LoginChallengeExpiryMinutes = 15;

        public AuthService(
            AppDbContext context,
            IConfiguration config,
            IEmailService emailService,
            IAuditLogService auditLogService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
            _auditLogService = auditLogService;
            _logger = logger;
        }
        // ================================================================
        // Registration / Email confirmation (unchanged)
        // ================================================================

        public async Task<string?> Register(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return null;

            var confirmationToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32));

            var confirmationTokenHash = Hash(confirmationToken);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                EmailConfirmed = false,
                EmailConfirmationTokenHash = confirmationTokenHash,
                EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            try
            {
                var webBaseUrl = _config["App:WebBaseUrl"];
                if (string.IsNullOrWhiteSpace(webBaseUrl))
                    throw new InvalidOperationException("App:WebBaseUrl is not configured.");

                var confirmationUrl =
                    $"{webBaseUrl.TrimEnd('/')}/Account/ConfirmEmail?token=" +
                    Uri.EscapeDataString(confirmationToken);

                var safeFullName = System.Net.WebUtility.HtmlEncode(user.FullName);

                var emailBody = $"""
            <!DOCTYPE html>
            <html>
            <head><meta charset="UTF-8"><title>Confirm your ECommerce account</title></head>
            <body style="margin:0;padding:0;background:#f5f5f5;font-family:Arial,sans-serif;">
                <div style="max-width:600px;margin:40px auto;background:white;padding:40px;border-radius:10px;">
                    <h1 style="margin-top:0;">Welcome to ECommerce</h1>
                    <p>Hello {safeFullName},</p>
                    <p>Thank you for creating your ECommerce account. We're glad to have you with us.</p>
                    <p>Before you can start shopping, we need to confirm this is really your email address. Please confirm your email by clicking the button below.</p>
                    <p style="margin:30px 0;">
                        <a href="{confirmationUrl}"
                           style="display:inline-block;padding:12px 24px;background:#198754;color:white;text-decoration:none;border-radius:6px;font-weight:bold;">
                            Confirm Email
                        </a>
                    </p>
                    <p style="color:#555;font-size:13px;">
                        If the button above doesn't work, copy and paste this link into your browser:<br>
                        {confirmationUrl}
                    </p>
                    <p>This confirmation link will expire in 24 hours. If it expires, you can request a new one from the login page.</p>
                    <p style="color:#777;font-size:13px;">
                        If you did not create this account, you can safely ignore this email and no further action is required.
                    </p>
                </div>
            </body>
            </html>
            """;

                await _emailService.SendAsync(
                    user.Email,
                    "Confirm your ECommerce account",
                    emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
                throw;
            }

            return "Registration successful. Please check your email to confirm your account.";
        }

        public async Task<bool> ConfirmEmail(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            string tokenHash;
            try
            {
                tokenHash = Hash(token);
            }
            catch
            {
                return false;
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.EmailConfirmationTokenHash == tokenHash);

            if (user == null)
                return false;

            if (user.EmailConfirmationTokenExpiresAt == null ||
                user.EmailConfirmationTokenExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            user.EmailConfirmed = true;
            user.EmailConfirmationTokenHash = null;
            user.EmailConfirmationTokenExpiresAt = null;

            await _context.SaveChangesAsync();

            try
            {
                var safeFullName = System.Net.WebUtility.HtmlEncode(user.FullName);

                var welcomeBody = $"""
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset="UTF-8">
        <title>Welcome to ECommerce</title>
    </head>
    <body style="margin:0;padding:0;background:#f5f5f5;font-family:Arial,sans-serif;">
        <div style="max-width:600px;margin:40px auto;background:white;padding:40px;border-radius:10px;">
            <h1>Welcome to ECommerce!</h1>
            <p>Hello {safeFullName},</p>
            <p>Your email address has been successfully confirmed.</p>
            <p>Your ECommerce account is now ready to use. You can log in and start browsing
                our catalog, add items to your cart, and place orders right away.</p>
            <p>Thank you for joining us. If you ever have questions about your account or an order,
                you can reach us by replying to this email.</p>
        </div>
    </body>
    </html>
    """;

                await _emailService.SendAsync(user.Email, "Welcome to ECommerce", welcomeBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", user.Email);
            }

            return true;
        }

        // ================================================================
        // Login (two steps for everyone: email OTP, then TOTP if enabled)
        // ================================================================

        public async Task<AuthResult> Login(LoginDto dto, string? ipAddress = null)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return AuthResult.Fail();

            // Already locked out — don't even check the password.
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
                return AuthResult.Locked(user.LockoutEnd.Value);

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                await RegisterFailedLoginAsync(user);

                await _auditLogService.LogAsync(
                    action: "LoginSucceeded",
                    entityName: "User",
                    entityId: user.Id.ToString(),
                    details: "User logged in successfully.",
                    userId: user.Id,
                    userEmail: user.Email,
                    ipAddress: ipAddress);

                return AuthResult.Fail();
            }
            if (!user.EmailConfirmed)
                return AuthResult.Fail();

            // Successful password check — clear any lockout state.
            if (user.FailedLoginAttempts != 0 || user.LockoutEnd != null)
            {
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();
            }

            // Every user, TOTP or not, gets an emailed code as step 1.
            await IssueLoginOtpAsync(user);
            var challengeToken = await IssueLoginChallengeAsync(user);

            return AuthResult.EmailOtpRequired(challengeToken, MaskEmail(user.Email));
        }

        // Step 1: confirm the emailed code.
        public async Task<AuthResult> VerifyLoginEmailOtp(ChallengeCodeDto dto, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(dto.ChallengeToken) || string.IsNullOrWhiteSpace(dto.Code))
                return AuthResult.Fail();

            var tokenHash = Hash(dto.ChallengeToken);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.LoginChallengeTokenHash == tokenHash);

            if (user == null)
                return AuthResult.Fail();

            if (user.LoginChallengeTokenExpiresAt == null ||
                user.LoginChallengeTokenExpiresAt < DateTime.UtcNow)
            {
                return AuthResult.Fail();
            }

            if (!await TryConsumeLoginOtpAsync(user, dto.Code))
                return AuthResult.Fail();

            user.LoginChallengeEmailVerified = true;
            await _context.SaveChangesAsync();

            // TOTP accounts need a second step; the same challenge token carries over.
            if (user.TwoFactorEnabled)
                return AuthResult.TotpRequired(dto.ChallengeToken);

            ClearLoginChallenge(user);
            await _context.SaveChangesAsync();

            return await IssueTokensAsync(user, ipAddress);
        }

        // Step 2 (only for accounts with TOTP enabled): confirm the authenticator code.
        public async Task<AuthResult> VerifyLoginTotp(ChallengeCodeDto dto, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(dto.ChallengeToken) || string.IsNullOrWhiteSpace(dto.Code))
                return AuthResult.Fail();

            var tokenHash = Hash(dto.ChallengeToken);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.LoginChallengeTokenHash == tokenHash);

            if (user == null)
                return AuthResult.Fail();

            if (user.LoginChallengeTokenExpiresAt == null ||
                user.LoginChallengeTokenExpiresAt < DateTime.UtcNow)
            {
                return AuthResult.Fail();
            }

            // Can't skip straight to TOTP without completing the email step first.
            if (!user.LoginChallengeEmailVerified)
                return AuthResult.Fail();

            if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                return AuthResult.Fail();

            if (!VerifyTotpCode(user.TwoFactorSecret, dto.Code))
                return AuthResult.Fail();

            ClearLoginChallenge(user);
            await _context.SaveChangesAsync();

            return await IssueTokensAsync(user, ipAddress);
        }

        public async Task<AuthResult> RefreshToken(string rawToken, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
                return AuthResult.Fail();

            var hash = Hash(rawToken);

            var stored = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == hash);

            if (stored == null)
                return AuthResult.Fail();

            if (!stored.IsActive)
            {
                // Reuse of an already-revoked/expired token is a signal that
                // the token may have been stolen — revoke the whole chain.
                if (stored.RevokedAt != null && stored.ReplacedByTokenHash != null)
                {
                    await RevokeDescendantsAsync(stored);
                }
                return AuthResult.Fail();
            }

            // Rotate: revoke this one, issue a brand new refresh token.
            stored.RevokedAt = DateTime.UtcNow;
            stored.RevokedByIp = ipAddress;

            var newRaw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var newHash = Hash(newRaw);
            stored.ReplacedByTokenHash = newHash;

            _context.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = newHash,
                UserId = stored.UserId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays()),
                CreatedByIp = ipAddress
            });

            await _context.SaveChangesAsync();

            var access = GenerateAccessToken(stored.User);
            return AuthResult.Ok(access, newRaw);
        }

        public async Task<bool> RevokeRefreshToken(string rawToken, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
                return false;

            var hash = Hash(rawToken);
            var stored = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == hash);

            if (stored == null || !stored.IsActive)
                return false;

            stored.RevokedAt = DateTime.UtcNow;
            stored.RevokedByIp = ipAddress;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task RevokeDescendantsAsync(RefreshToken token)
        {
            var current = token;
            while (current.ReplacedByTokenHash != null)
            {
                var next = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.TokenHash == current.ReplacedByTokenHash);

                if (next == null)
                    break;

                if (next.IsActive)
                {
                    next.RevokedAt = DateTime.UtcNow;
                }

                current = next;
            }

            await _context.SaveChangesAsync();
        }

        private async Task RegisterFailedLoginAsync(User user)
        {
            var maxAttempts = GetMaxFailedLoginAttempts();
            var lockoutMinutes = GetLockoutMinutes();

            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= maxAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);
                user.FailedLoginAttempts = 0;

                await _auditLogService.LogAsync(
                    action: "AccountLockedOut",
                    entityName: "User",
                    entityId: user.Id.ToString(),
                    details: $"Account locked out until {user.LockoutEnd:u} after too many failed login attempts.",
                    userId: user.Id,
                    userEmail: user.Email);
            }

            await _context.SaveChangesAsync();

        }

        private async Task<AuthResult> IssueTokensAsync(User user, string? ipAddress)
        {
            var access = GenerateAccessToken(user);
            var refreshRaw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            _context.RefreshTokens.Add(new RefreshToken
            {
                TokenHash = Hash(refreshRaw),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays()),
                CreatedByIp = ipAddress
            });

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                    action: "LoginSucceeded",
                    entityName: "User",
                    entityId: user.Id.ToString(),
                    details: "User logged in successfully.",
                    userId: user.Id,
                    userEmail: user.Email,
                    ipAddress: ipAddress);

            return AuthResult.Ok(access, refreshRaw);
        }

        // ---- Login challenge + email OTP helpers ----

        private async Task<string> IssueLoginChallengeAsync(User user)
        {
            var challenge = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            user.LoginChallengeTokenHash = Hash(challenge);
            user.LoginChallengeTokenExpiresAt = DateTime.UtcNow.AddMinutes(LoginChallengeExpiryMinutes);
            user.LoginChallengeEmailVerified = false;

            await _context.SaveChangesAsync();
            return challenge;
        }

        private static void ClearLoginChallenge(User user)
        {
            user.LoginChallengeTokenHash = null;
            user.LoginChallengeTokenExpiresAt = null;
            user.LoginChallengeEmailVerified = false;
        }

        private async Task IssueLoginOtpAsync(User user)
        {
            var code = GenerateOtpCode();

            user.LoginOtpCodeHash = Hash(code);
            user.LoginOtpExpiresAt = DateTime.UtcNow.AddMinutes(LoginOtpExpiryMinutes);
            user.LoginOtpAttempts = 0;

            await _context.SaveChangesAsync();
            await SendLoginOtpEmailAsync(user, code);
        }

        private bool IsLoginOtpValid(User user, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            if (string.IsNullOrEmpty(user.LoginOtpCodeHash))
                return false;

            if (user.LoginOtpExpiresAt == null || user.LoginOtpExpiresAt < DateTime.UtcNow)
                return false;

            if (user.LoginOtpAttempts >= LoginOtpMaxAttempts)
                return false;

            return Hash(code.Trim()) == user.LoginOtpCodeHash;
        }

        // Validates the emailed code and consumes it (clears on success,
        // counts the attempt on failure) so it can't be replayed or brute-forced.
        private async Task<bool> TryConsumeLoginOtpAsync(User user, string code)
        {
            if (!IsLoginOtpValid(user, code))
            {
                var hasLiveOtp =
                    !string.IsNullOrEmpty(user.LoginOtpCodeHash) &&
                    user.LoginOtpExpiresAt != null &&
                    user.LoginOtpExpiresAt >= DateTime.UtcNow &&
                    user.LoginOtpAttempts < LoginOtpMaxAttempts;

                if (hasLiveOtp)
                {
                    user.LoginOtpAttempts++;
                    await _context.SaveChangesAsync();
                }

                return false;
            }

            user.LoginOtpCodeHash = null;
            user.LoginOtpExpiresAt = null;
            user.LoginOtpAttempts = 0;

            await _context.SaveChangesAsync();
            return true;
        }

        private static string GenerateOtpCode() =>
            RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        private static string MaskEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            if (atIndex <= 1)
                return email;

            return $"{email[0]}***{email[atIndex..]}";
        }

        private async Task SendLoginOtpEmailAsync(User user, string code)
        {
            var safeFullName = System.Net.WebUtility.HtmlEncode(user.FullName);

            var emailBody = $"""
            <!DOCTYPE html>
            <html>
            <head><meta charset="UTF-8"><title>Your ECommerce login code</title></head>
            <body style="margin:0;padding:0;background:#f5f5f5;font-family:Arial,sans-serif;">
                <div style="max-width:600px;margin:40px auto;background:white;padding:40px;border-radius:10px;">
                    <h1 style="margin-top:0;">Your login code</h1>
                    <p>Hello {safeFullName},</p>
                    <p>Use the code below to finish signing in.</p>
                    <p style="margin:30px 0;text-align:center;">
                        <span style="display:inline-block;padding:16px 28px;background:#f0f0f0;border-radius:8px;font-size:32px;font-weight:bold;letter-spacing:8px;">
                            {code}
                        </span>
                    </p>
                    <p>This code will expire in {LoginOtpExpiryMinutes} minutes.</p>
                    <p style="color:#777;font-size:13px;">
                        If you did not attempt to log in, you can safely ignore this email — someone may have entered your email address by mistake.
                    </p>
                </div>
            </body>
            </html>
            """;

            try
            {
                await _emailService.SendAsync(user.Email, "Your ECommerce login code", emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send login OTP email to {Email}", user.Email);
                throw;
            }
        }

        // ================================================================
        // Security: TOTP two-factor authentication (opt-in, managed from
        // the Security page — separate from the mandatory login email OTP)
        // ================================================================

        // Step 1 of enabling TOTP: generate a secret and return the QR data.
        public async Task<Setup2FaResponseDto?> SetupTotp(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return null;

            var secretBytes = KeyGeneration.GenerateRandomKey(20);
            var base32Secret = Base32Encoding.ToString(secretBytes);

            // Stored right away so Enable() can verify against it, but
            // TwoFactorEnabled stays false until the user confirms a code.
            user.TwoFactorSecret = base32Secret;
            await _context.SaveChangesAsync();

            const string issuer = "ECommerce";
            var label = Uri.EscapeDataString($"{issuer}:{user.Email}");
            var qrCodeUri =
                $"otpauth://totp/{label}?secret={base32Secret}&issuer={Uri.EscapeDataString(issuer)}&digits=6&period=30";

            return new Setup2FaResponseDto
            {
                SecretKey = base32Secret,
                QrCodeUri = qrCodeUri
            };
        }

        // Step 2 of enabling TOTP: confirm a code generated from the secret above.
        public async Task<bool> EnableTotp(int userId, string code)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
                return false;

            if (!VerifyTotpCode(user.TwoFactorSecret, code))
                return false;

            user.TwoFactorEnabled = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DisableTotp(int userId, string code)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                return false;

            if (!VerifyTotpCode(user.TwoFactorSecret, code))
                return false;

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsTotpEnabled(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.TwoFactorEnabled ?? false;
        }

        private static bool VerifyTotpCode(string base32Secret, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            try
            {
                var keyBytes = Base32Encoding.ToBytes(base32Secret);
                var totp = new Totp(keyBytes);

                // Tolerate the previous/next 30s window for clock drift.
                return totp.VerifyTotp(code.Trim(), out _, new VerificationWindow(previous: 1, future: 1));
            }
            catch
            {
                return false;
            }
        }

        // ================================================================
        // Forgot / reset password (unchanged)
        // ================================================================

        public async Task ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return;

            email = email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null)
                return;

            var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            user.PasswordResetTokenHash = Hash(resetToken);
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            var webBaseUrl = _config["App:WebBaseUrl"];
            if (string.IsNullOrWhiteSpace(webBaseUrl))
                throw new InvalidOperationException("App:WebBaseUrl is not configured.");

            var resetUrl =
                $"{webBaseUrl.TrimEnd('/')}/Account/ResetPassword?token=" +
                Uri.EscapeDataString(resetToken);

            var safeFullName = System.Net.WebUtility.HtmlEncode(user.FullName);

            var emailBody = $"""
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Reset your ECommerce password</title>
</head>
<body style="margin:0;padding:0;background:#f5f5f5;font-family:Arial,sans-serif;">
<div style="max-width:600px;margin:40px auto;background:white;padding:40px;border-radius:10px;">
    <h1 style="margin-top:0;">Reset your password</h1>
    <p>Hello {safeFullName},</p>
    <p>We received a request to reset the password for your ECommerce account.</p>
    <p>Click the button below to create a new password.</p>
    <p style="margin:30px 0;">
        <a href="{resetUrl}"
           style="display:inline-block;padding:12px 24px;background:#198754;color:white;text-decoration:none;border-radius:6px;font-weight:bold;">
            Reset Password
        </a>
    </p>
    <p>This password reset link will expire in 1 hour.</p>
    <p style="color:#555;font-size:13px;">
        If the button doesn't work, copy and paste this link into your browser:
    </p>
    <p style="color:#777;font-size:13px;word-break:break-all;">{resetUrl}</p>
    <p style="color:#777;font-size:13px;">
        If you did not request a password reset, you can safely ignore this email.
    </p>
</div>
</body>
</html>
""";

            try
            {
                await _emailService.SendAsync(user.Email, "Reset your ECommerce password", emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
                throw;
            }
        }

        public async Task<bool> ResetPassword(ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                return false;

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return false;

            if (dto.NewPassword != dto.ConfirmPassword)
                return false;

            string tokenHash;
            try
            {
                tokenHash = Hash(dto.Token);
            }
            catch
            {
                return false;
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetTokenHash == tokenHash);

            if (user == null)
                return false;

            if (user.PasswordResetTokenExpiresAt == null ||
                user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresAt = null;

            await _context.SaveChangesAsync();
            return true;
        }

        // ================================================================
        // Helpers
        // ================================================================

        private static string Hash(string input) =>
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(input)));

        private int GetMaxFailedLoginAttempts() =>
            int.TryParse(_config["Security:MaxFailedLoginAttempts"], out var v) ? v : 5;

        private int GetLockoutMinutes() =>
            int.TryParse(_config["Security:LockoutMinutes"], out var v) ? v : 15;

        private int GetAccessTokenExpiryMinutes() =>
            int.TryParse(_config["Jwt:AccessTokenExpiryMinutes"], out var v) ? v : 15;

        private int GetRefreshTokenExpiryDays() =>
            int.TryParse(_config["Jwt:RefreshTokenExpiryDays"], out var v) ? v : 30;

        private string GenerateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}