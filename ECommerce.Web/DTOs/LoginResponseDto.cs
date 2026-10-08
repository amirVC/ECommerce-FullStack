namespace ECommerce.Web.DTOs;

public class LoginResponseDto
{
    public bool RequiresEmailOtp { get; set; }
    public bool RequiresTotp { get; set; }
    public string? ChallengeToken { get; set; }
    public string? MaskedEmail { get; set; }

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}