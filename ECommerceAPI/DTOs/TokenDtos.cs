namespace ECommerceAPI.DTOs
{
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    // Unified shape returned by /login, /login/verify-email-otp,
    // /login/verify-totp and /refresh.
    //
    // - RequiresEmailOtp: every login stops here first. Call
    //   /login/verify-email-otp with ChallengeToken + the emailed code.
    // - RequiresTotp: only set if the account has TOTP enabled. Call
    //   /login/verify-totp with the SAME ChallengeToken + authenticator code.
    // - Otherwise AccessToken/RefreshToken are set and login is complete.
    public class AuthResponseDto
    {
        public bool RequiresEmailOtp { get; set; } = false;
        public bool RequiresTotp { get; set; } = false;
        public string? ChallengeToken { get; set; }
        public string? MaskedEmail { get; set; }

        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}