namespace ECommerceAPI.Models;

public class Campaign
{
    public int Id { get; set; }

    // Public API identifier.
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public CampaignDiscountType DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<CampaignProduct> CampaignProducts { get; set; }
        = new List<CampaignProduct>();

    public ICollection<CampaignCategory> CampaignCategories { get; set; }
        = new List<CampaignCategory>();
}