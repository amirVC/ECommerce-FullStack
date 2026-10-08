namespace ECommerce.Web.DTOs;
public class AdminProductDto
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }   
    public string CategoryName { get; set; } = string.Empty;
    public decimal? SalePrice { get; set; }
    public DateTime? SaleStartDate { get; set; }
    public DateTime? SaleEndDate { get; set; }
    public bool IsOnSale { get; set; }
    public decimal EffectivePrice { get; set; }
}