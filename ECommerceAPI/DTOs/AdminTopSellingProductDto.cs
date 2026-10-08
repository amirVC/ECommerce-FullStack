namespace ECommerceAPI.DTOs;

public class AdminTopSellingProductDto
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int QuantitySold { get; set; }

    public decimal Revenue { get; set; }
}