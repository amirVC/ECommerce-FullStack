namespace ECommerce.Web.DTOs;

public class TotpSetupResponseDto
{
    public string SecretKey { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
}