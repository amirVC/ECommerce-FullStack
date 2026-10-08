using ECommerceAPI.DTOs;

public class ProductDto
{
    public int Id { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string? ImageUrl { get; set; }

    public string? ThumbnailUrl { get; set; }  

    public int CategoryId { get; set; }   

    public List<ProductImageDto> Images { get; set; } = new();

    // Sale
    public decimal? SalePrice { get; set; }

    public DateTime? SaleStartDate { get; set; }

    public DateTime? SaleEndDate { get; set; }

    public bool IsOnSale { get; set; }

    public decimal EffectivePrice { get; set; }

    // Campaign
    public decimal CampaignDiscount { get; set; }

    public bool HasCampaign { get; set; }

    public string? CampaignName { get; set; }

    // Final pricing
    public decimal FinalPrice { get; set; }

    public decimal TotalDiscountAmount { get; set; }

    // Reviews
    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }
}