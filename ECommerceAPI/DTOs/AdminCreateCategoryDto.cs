using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs;

public class AdminCreateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}