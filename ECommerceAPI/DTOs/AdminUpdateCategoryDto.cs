
using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs;

public class AdminUpdateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [RegularExpression(
        @"^[a-z0-9]+(?:-[a-z0-9]+)*$",
        ErrorMessage = "Slug must contain lowercase letters, numbers, and single hyphens only.")]
    public string Slug { get; set; } = string.Empty;
}
