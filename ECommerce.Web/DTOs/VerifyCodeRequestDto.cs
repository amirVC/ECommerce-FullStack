namespace ECommerce.Web.DTOs;

// Submitted for both the email-OTP and TOTP login steps — same shape either way.
public class VerifyCodeRequestDto
{
    public string ChallengeToken { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}