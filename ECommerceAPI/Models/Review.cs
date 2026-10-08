namespace ECommerceAPI.Models
{
    public enum ReviewStatus
    {
        Approved = 0,
        Hidden = 1
    }

    public class Review
    {
        public int Id { get; set; }

        // Public API identifier.
        public Guid PublicId { get; set; } = Guid.NewGuid();

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int Rating { get; set; }
        public string? Title { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }   

        public bool IsVerifiedPurchase { get; set; }
        public ReviewStatus Status { get; set; } = ReviewStatus.Approved;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}