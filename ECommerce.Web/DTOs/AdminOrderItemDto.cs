namespace ECommerce.Web.DTOs;

public class AdminOrderItemDto
{
    public string ProductName { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public decimal Total => Price * Quantity;
}