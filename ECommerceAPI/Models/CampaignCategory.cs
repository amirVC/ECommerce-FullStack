namespace ECommerceAPI.Models;

public class CampaignCategory
{
    public int CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;
}