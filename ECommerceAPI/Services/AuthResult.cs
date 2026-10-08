namespace ECommerceAPI.Services
{
    public class AuthResult
    {
        public bool Success { get; set; }

        public bool RequiresEmailOtp { get; set; }
        public bool RequiresTotp { get; set; }

        public bool AccountLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }

        public string? ChallengeToken { get; set; }
        public string? MaskedEmail { get; set; }

        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

        public static AuthResult Fail() => new() { Success = false };

        public static AuthResult Locked(DateTime until) => new()
        {
            Success = false,
            AccountLocked = true,
            LockoutEnd = until
        };

        public static AuthResult EmailOtpRequired(string challengeToken, string maskedEmail) => new()
        {
            Success = false,
            RequiresEmailOtp = true,
            ChallengeToken = challengeToken,
            MaskedEmail = maskedEmail
        };

        public static AuthResult TotpRequired(string challengeToken) => new()
        {
            Success = false,
            RequiresTotp = true,
            ChallengeToken = challengeToken
        };

        public static AuthResult Ok(string accessToken, string refreshToken) => new()
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}