namespace ECommerceAPI.DTOs
{
    public class WishlistItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string? ProductThumbnailUrl { get; set; }  
        public decimal Price { get; set; }
        public int AvailableStock { get; set; }
        public bool InStock { get; set; }
        public DateTime AddedAt { get; set; }

        // Final price after sale + campaign
        public decimal FinalPrice { get; set; }

        // Sale discount amount
        public decimal SaleDiscountAmount { get; set; }

        // Campaign discount amount
        public decimal CampaignDiscountAmount { get; set; }

        // Sale + campaign
        public decimal TotalDiscountAmount { get; set; }

        // Total percentage saved from original price
        public decimal TotalDiscountPercentage { get; set; }

        public bool IsOnSale { get; set; }

        public bool HasCampaign { get; set; }

        public string? CampaignName { get; set; }

    }
}