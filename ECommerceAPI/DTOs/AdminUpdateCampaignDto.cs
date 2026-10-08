namespace ECommerceAPI.DTOs;

public class AdminUpdateCampaignDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public List<int> ProductIds { get; set; } = new();

    public List<int> CategoryIds { get; set; } = new();
}