namespace ECommerceAPI.Models
{
    public class Wishlist
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }
}