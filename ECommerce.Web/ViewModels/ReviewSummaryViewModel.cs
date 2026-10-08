namespace ECommerce.Web.ViewModels;

public class ReviewSummaryViewModel
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingBreakdown { get; set; } = new();
}