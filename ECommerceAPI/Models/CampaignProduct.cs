namespace ECommerceAPI.Models;

public class CampaignProduct
{
    public int CampaignId { get; set; }

    public Campaign Campaign { get; set; } = null!;

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;
}