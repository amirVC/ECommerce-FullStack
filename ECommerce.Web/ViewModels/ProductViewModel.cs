namespace ECommerce.Web.ViewModels;

public class ProductViewModel
{
    public int Id { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Original product price
    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string? ImageUrl { get; set; }

    public string? ThumbnailUrl { get; set; }   // NEW

    public string CategoryName { get; set; } = string.Empty;

    public List<ProductImageViewModel> Images { get; set; } = new();

    // Reviews
    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }

    // --------------------------------------------------
    // SALE PRICING
    // --------------------------------------------------

    public decimal? SalePrice { get; set; }

    public DateTime? SaleStartDate { get; set; }

    public DateTime? SaleEndDate { get; set; }

    public bool IsOnSale { get; set; }

    // Price after product sale
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

    // Final customer price after Sale + Campaign
    public decimal FinalPrice { get; set; }

    // Total discount compared to original Price
    public decimal TotalDiscountAmount { get; set; }

    // --------------------------------------------------
    // HELPER PROPERTIES FOR UI
    // --------------------------------------------------

    public bool HasAnyDiscount =>
        IsOnSale || HasCampaign || TotalDiscountAmount > 0;

    public int TotalDiscountPercentage
    {
        get
        {
            if (Price <= 0 || TotalDiscountAmount <= 0)
                return 0;

            return (int)Math.Round(
                (TotalDiscountAmount / Price) * 100
            );
        }
    }
}