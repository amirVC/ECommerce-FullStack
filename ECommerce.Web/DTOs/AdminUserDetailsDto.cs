namespace ECommerce.Web.DTOs;

public class AdminUserDetailsDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public int OrdersCount { get; set; }

    public decimal TotalSpent { get; set; }

    public List<AdminUserOrderDto> Orders { get; set; } = new();
}