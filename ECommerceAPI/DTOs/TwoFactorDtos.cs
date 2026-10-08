namespace ECommerceAPI.DTOs
{
    // Submitted for both /login/verify-email-otp and /login/verify-totp —
    // same shape, different code source (emailed code vs. authenticator app).
    public class ChallengeCodeDto
    {
        public string ChallengeToken { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    // Returned by /security/2fa/setup.
    public class Setup2FaResponseDto
    {
        // Base32 secret, shown once so the user can enter it manually
        // into their authenticator app if they can't scan the QR code.
        public string SecretKey { get; set; } = string.Empty;

        // otpauth:// URI — render this as a QR code on the client.
        public string QrCodeUri { get; set; } = string.Empty;
    }

    // Confirms a 6-digit authenticator code, e.g. when enabling TOTP.
    public class TwoFactorVerifyDto
    {
        public string Code { get; set; } = string.Empty;
    }

    public class Disable2FaDto
    {
        public string Code { get; set; } = string.Empty;
    }

    public class TotpStatusResponseDto
    {
        public bool Enabled { get; set; }
    }
}