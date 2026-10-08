namespace ECommerceAPI.DTOs;

public class AdminOrderDto
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public int ItemCount { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}