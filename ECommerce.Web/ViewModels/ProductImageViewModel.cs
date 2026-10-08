namespace ECommerce.Web.ViewModels;

public class ProductImageViewModel
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string AltText { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }

    public string? ThumbnailUrl { get; set; }
}