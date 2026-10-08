namespace ECommerceAPI.Models;

public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public string ImageUrl { get; set; } = string.Empty;

    public string ThumbnailUrl { get; set; } = string.Empty;   

    public bool IsPrimary { get; set; }

    public string AltText { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}