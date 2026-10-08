using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.ViewModels;

public class TwoFactorLoginViewModel
{
    [Required]
    public string ChallengeToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the 6-digit code we emailed you.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "The code must be 6 digits.")]
    public string Code { get; set; } = string.Empty;
}