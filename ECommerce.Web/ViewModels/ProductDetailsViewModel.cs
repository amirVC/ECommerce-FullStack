using ECommerce.Web.DTOs;
using ECommerce.Web.ViewModels;

namespace ECommerce.Web.ViewModels;

public class ProductDetailsViewModel
{
    public ProductViewModel Product { get; set; } = new();
    public List<ProductViewModel> RelatedProducts { get; set; } = new();
    public ReviewSummaryViewModel ReviewSummary { get; set; } = new();
    public List<ReviewViewModel> Reviews { get; set; } = new();
    public bool CanReview { get; set; }
    public ReviewViewModel? MyReview { get; set; }
    public List<RecentlyViewedDto> RecentlyViewed { get; set; } = new();
}