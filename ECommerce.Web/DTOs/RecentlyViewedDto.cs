namespace ECommerce.Web.DTOs;

public class RecentlyViewedDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public string? ThumbnailUrl { get; set; }   

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }

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
}