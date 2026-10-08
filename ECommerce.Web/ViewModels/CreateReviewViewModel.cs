using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.ViewModels;

public class CreateReviewViewModel
{
    public int ProductId { get; set; }

    [Range(1, 5, ErrorMessage = "Please select a rating.")]
    public int Rating { get; set; }

    [StringLength(150)]
    public string? Title { get; set; }

    [Required, StringLength(2000, MinimumLength = 10)]
    public string Comment { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }
}