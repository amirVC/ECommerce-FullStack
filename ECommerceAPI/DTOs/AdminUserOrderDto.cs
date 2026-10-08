namespace ECommerceAPI.DTOs;

public class AdminUserOrderDto
{
    public int Id { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }
}