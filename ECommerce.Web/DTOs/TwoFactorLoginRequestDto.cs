namespace ECommerce.Web.DTOs;

public class TwoFactorLoginRequestDto
{
    public string ChallengeToken { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}