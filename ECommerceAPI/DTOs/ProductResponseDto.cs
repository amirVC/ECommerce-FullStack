using ECommerceAPI.DTOs;

namespace ECommerceAPI.DTOs;

public class ProductResponseDto
{
    public int Id { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Original price
    public decimal Price { get; set; }

    public int Stock { get; set; }

    // Keep for compatibility
    public string? ImageUrl { get; set; }

    public string? ThumbnailUrl { get; set; }  

    public string CategoryName { get; set; } = string.Empty;

    public List<ProductImageDto> Images { get; set; } = new();

    // Reviews
    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }

    // --------------------------------------------------
    // SALE
    // --------------------------------------------------

    public decimal? SalePrice { get; set; }

    public DateTime? SaleStartDate { get; set; }

    public DateTime? SaleEndDate { get; set; }

    public bool IsOnSale { get; set; }

    // Price after Sale
    public decimal EffectivePrice { get; set; }

    // --------------------------------------------------
    // CAMPAIGN
    // --------------------------------------------------

    public decimal CampaignDiscount { get; set; }

    public bool HasCampaign { get; set; }

    public string? CampaignName { get; set; }

    // --------------------------------------------------
    // FINAL PRICE
    // --------------------------------------------------

    // Final price after Sale + Campaign
    public decimal FinalPrice { get; set; }

    // Original Price - Final Price
    public decimal TotalDiscountAmount { get; set; }
}