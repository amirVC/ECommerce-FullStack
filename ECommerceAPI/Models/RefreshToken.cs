namespace ECommerceAPI.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        // SHA-256 hash of the raw token. The raw token is only ever
        // returned to the client once and never stored.
        public string TokenHash { get; set; } = string.Empty;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        // Set when this token was rotated into a new one, so a reuse of
        // a revoked token can be detected as a possible theft attempt.
        public string? ReplacedByTokenHash { get; set; }

        public string? CreatedByIp { get; set; }
        public string? RevokedByIp { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => RevokedAt == null && !IsExpired;
    }
}