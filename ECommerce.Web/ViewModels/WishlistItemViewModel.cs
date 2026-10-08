namespace ECommerce.Web.ViewModels;

public class WishlistItemViewModel
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public string? ThumbnailUrl { get; set; }   // NEW

    // Original/main price
    public decimal Price { get; set; }

    // Price after sale + campaign
    public decimal FinalPrice { get; set; }

    // Sale discount
    public decimal SaleDiscountAmount { get; set; }

    // Campaign discount
    public decimal CampaignDiscountAmount { get; set; }

    // Total discount
    public decimal TotalDiscountAmount { get; set; }

    // Total discount percentage
    public decimal TotalDiscountPercentage { get; set; }

    public bool IsOnSale { get; set; }

    public bool HasCampaign { get; set; }

    public bool HasAnyDiscount =>
        IsOnSale || HasCampaign;

    public string? CampaignName { get; set; }

    public int AvailableStock { get; set; }

    public bool InStock { get; set; }

    public DateTime AddedAt { get; set; }
}