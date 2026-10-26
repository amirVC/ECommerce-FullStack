namespace ECommerce.Web.ViewModels;

public class RecentlyViewedItemViewModel
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }   
    public DateTime ViewedAt { get; set; }
}