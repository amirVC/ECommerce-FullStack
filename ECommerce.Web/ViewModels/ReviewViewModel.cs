namespace ECommerce.Web.ViewModels;

public class ReviewViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public DateTime CreatedAt { get; set; }
    public double AverageRating { get; set; }  
    public int ReviewCount { get; set; }        
}