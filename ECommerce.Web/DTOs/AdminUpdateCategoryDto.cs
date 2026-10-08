using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.DTOs;

public class AdminUpdateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}