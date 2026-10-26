namespace ECommerce.Web.ViewModels;

public class WishlistItemViewModel
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public string? ProductSlug { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public string? ThumbnailUrl { get; set; }   

    public decimal Price { get; set; }

    public decimal FinalPrice { get; set; }

    public decimal SaleDiscountAmount { get; set; }

    public decimal CampaignDiscountAmount { get; set; }

    public decimal TotalDiscountAmount { get; set; }

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