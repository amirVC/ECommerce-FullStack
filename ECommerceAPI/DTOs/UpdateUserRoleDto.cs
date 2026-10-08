using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs;

public class UpdateUserRoleDto
{
    [Required]
    public string Role { get; set; } = string.Empty;
}