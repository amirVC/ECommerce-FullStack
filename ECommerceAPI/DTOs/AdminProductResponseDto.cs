namespace ECommerceAPI.DTOs;
public class AdminProductResponseDto
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;

    // Sale pricing
    public decimal? SalePrice { get; set; }
    public DateTime? SaleStartDate { get; set; }
    public DateTime? SaleEndDate { get; set; }
    public bool IsOnSale { get; set; }
    public decimal EffectivePrice { get; set; }
}