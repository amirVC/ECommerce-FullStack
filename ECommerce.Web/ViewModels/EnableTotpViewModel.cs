using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.ViewModels;

public class EnableTotpViewModel
{
    public string SecretKey { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the 6-digit code from your authenticator app.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "The code must be 6 digits.")]
    public string Code { get; set; } = string.Empty;
}