namespace ECommerceAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        // Public API identifier. Keep Id internal for EF relationships.
        public Guid PublicId { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer"; // Admin or Customer
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool EmailConfirmed { get; set; } = false;
        public string? EmailConfirmationTokenHash { get; set; }
        public DateTime? EmailConfirmationTokenExpiresAt { get; set; }

        public string? PasswordResetTokenHash { get; set; }
        public DateTime? PasswordResetTokenExpiresAt { get; set; }

        // ---- Account lockout ----
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }

        // ---- Login step 1: email OTP, required for every user on every login ----
        public string? LoginOtpCodeHash { get; set; }
        public DateTime? LoginOtpExpiresAt { get; set; }
        public int LoginOtpAttempts { get; set; } = 0;

        // Spans both login steps (email OTP, then TOTP if the account has it enabled).
        public string? LoginChallengeTokenHash { get; set; }
        public DateTime? LoginChallengeTokenExpiresAt { get; set; }
        public bool LoginChallengeEmailVerified { get; set; } = false;

        // ---- Two-factor authentication (TOTP), opt-in from the Security page ----
        public bool TwoFactorEnabled { get; set; } = false;
        public string? TwoFactorSecret { get; set; }

        // Navigation
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<RecentlyViewed> RecentlyViewedProducts { get; set; } = new List<RecentlyViewed>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}