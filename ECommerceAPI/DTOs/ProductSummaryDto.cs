namespace ECommerceAPI.DTOs;
public class ProductSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }

    // Sale pricing
    public bool IsOnSale { get; set; }
    public decimal EffectivePrice { get; set; }
}