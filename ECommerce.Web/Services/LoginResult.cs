namespace ECommerce.Web.Services;

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    AccountLocked,
    RequiresEmailOtp,
    RequiresTotp
}

public class LoginResult
{
    public LoginStatus Status { get; set; }
    public string? ChallengeToken { get; set; }
    public string? MaskedEmail { get; set; }
    public string? LockoutMessage { get; set; }

    public static LoginResult Success() => new() { Status = LoginStatus.Success };

    public static LoginResult Invalid() => new() { Status = LoginStatus.InvalidCredentials };

    public static LoginResult Locked(string message) => new()
    {
        Status = LoginStatus.AccountLocked,
        LockoutMessage = message
    };

    public static LoginResult EmailOtpRequired(string challengeToken, string? maskedEmail) => new()
    {
        Status = LoginStatus.RequiresEmailOtp,
        ChallengeToken = challengeToken,
        MaskedEmail = maskedEmail
    };

    public static LoginResult TotpRequired(string challengeToken) => new()
    {
        Status = LoginStatus.RequiresTotp,
        ChallengeToken = challengeToken
    };
}