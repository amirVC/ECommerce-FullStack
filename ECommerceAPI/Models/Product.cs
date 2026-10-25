namespace ECommerceAPI.Models
{
    public class Product
    {
        public int Id { get; set; }

        public Guid PublicId { get; set; } = Guid.NewGuid();

        public string SKU { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public decimal? SalePrice { get; set; }
        public DateTime? SaleStartDate { get; set; }
        public DateTime? SaleEndDate { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<RecentlyViewed> RecentlyViewedByUsers { get; set; } = new List<RecentlyViewed>();
    }
}