using ECommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<RecentlyViewed> RecentlyViewed { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponProduct> CouponProducts { get; set; }
        public DbSet<CouponCategory> CouponCategories { get; set; }
        public DbSet<CouponUsage> CouponUsages { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignProduct> CampaignProducts { get; set; }
        public DbSet<CampaignCategory> CampaignCategories { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<RefundRequest> RefundRequests { get; set; }
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<DeliveryMethod> DeliveryMethods => Set<DeliveryMethod>();
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<ShipmentStatusHistory> ShipmentStatusHistories => Set<ShipmentStatusHistory>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("pg_trgm");

            // ============================================================
            // PUBLIC IDs
            // ============================================================
            //
            // PublicId is exposed through the API.
            // Id remains the internal database primary key.
            //
            // These indexes must be unique because PublicId is used to
            // identify resources at the API boundary.
            // ============================================================

            modelBuilder.Entity<User>()
                .HasIndex(u => u.PublicId)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.PublicId)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.PublicId)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.PublicId)
                .IsUnique();

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.PublicId)
                .IsUnique();

            modelBuilder.Entity<Address>()
                .HasIndex(a => a.PublicId)
                .IsUnique();

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.PublicId)
                .IsUnique();

            modelBuilder.Entity<RefundRequest>()
                .HasIndex(r => r.PublicId)
                .IsUnique();

            modelBuilder.Entity<Shipment>()
                .HasIndex(s => s.PublicId)
                .IsUnique();

            modelBuilder.Entity<Campaign>()
                .HasIndex(c => c.PublicId)
                .IsUnique();

            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.PublicId)
                .IsUnique();

            modelBuilder.Entity<DeliveryMethod>()
                .HasIndex(d => d.PublicId)
                .IsUnique();


            // ============================================================
            // User
            // ============================================================

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();


            // ============================================================
            // Product → Category
            // ============================================================

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // Product SKU
            modelBuilder.Entity<Product>()
                .Property(p => p.SKU)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();


            // ============================================================
            // Order → User
            // ============================================================

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Status);

            modelBuilder.Entity<Order>()
                .HasIndex(o => new { o.Status, o.OrderDate });


            // ============================================================
            // OrderItem → Order
            // ============================================================

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);


            // ============================================================
            // OrderItem → Product
            // ============================================================

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);


            // ============================================================
            // Decimal precision
            // ============================================================

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);


            // Product stock
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Stock);


            // Product name search
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .HasMethod("gin")
                .HasOperators("gin_trgm_ops");


            // ============================================================
            // Cart → User
            // ============================================================

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .IsUnique();


            // ============================================================
            // CartItem → Cart
            // ============================================================

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId);


            // ============================================================
            // CartItem → Product
            // ============================================================

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId);


            // One row per product per cart
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => new { ci.CartId, ci.ProductId })
                .IsUnique();


            // ============================================================
            // ProductImage → Product
            // ============================================================

            modelBuilder.Entity<ProductImage>()
                .HasOne(i => i.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================================
            // Wishlist → User
            // ============================================================

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId);

            modelBuilder.Entity<Wishlist>()
                .HasIndex(w => w.UserId)
                .IsUnique();


            // ============================================================
            // WishlistItem → Wishlist
            // ============================================================

            modelBuilder.Entity<WishlistItem>()
                .HasOne(wi => wi.Wishlist)
                .WithMany(w => w.WishlistItems)
                .HasForeignKey(wi => wi.WishlistId);


            // ============================================================
            // WishlistItem → Product
            // ============================================================

            modelBuilder.Entity<WishlistItem>()
                .HasOne(wi => wi.Product)
                .WithMany()
                .HasForeignKey(wi => wi.ProductId);


            // One row per product per wishlist
            modelBuilder.Entity<WishlistItem>()
                .HasIndex(wi => new { wi.WishlistId, wi.ProductId })
                .IsUnique();


            // ============================================================
            // Review
            // ============================================================

            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.Product)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(r => new { r.ProductId, r.UserId })
                    .IsUnique();

                entity.Property(r => r.Rating)
                    .IsRequired();

                entity.Property(r => r.Comment)
                    .HasMaxLength(2000);

                entity.Property(r => r.Title)
                    .HasMaxLength(150);
            });


            // ============================================================
            // Recently Viewed
            // ============================================================

            modelBuilder.Entity<RecentlyViewed>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.RecentlyViewedProducts)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Product)
                    .WithMany(x => x.RecentlyViewedByUsers)
                    .HasForeignKey(x => x.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.ProductId
                }).IsUnique();

                entity.HasIndex(x => new
                {
                    x.UserId,
                    x.ViewedAt
                });
            });


            // ============================================================
            // Coupon
            // ============================================================

            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<Coupon>()
                .Property(c => c.DiscountValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Coupon>()
                .Property(c => c.MaxDiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Coupon>()
                .Property(c => c.MinimumOrderAmount)
                .HasPrecision(18, 2);


            // ============================================================
            // CouponProduct
            // ============================================================

            modelBuilder.Entity<CouponProduct>()
                .HasKey(cp => new { cp.CouponId, cp.ProductId });

            modelBuilder.Entity<CouponProduct>()
                .HasOne(cp => cp.Coupon)
                .WithMany(c => c.CouponProducts)
                .HasForeignKey(cp => cp.CouponId);

            modelBuilder.Entity<CouponProduct>()
                .HasOne(cp => cp.Product)
                .WithMany()
                .HasForeignKey(cp => cp.ProductId);


            // ============================================================
            // CouponCategory
            // ============================================================

            modelBuilder.Entity<CouponCategory>()
                .HasKey(cc => new { cc.CouponId, cc.CategoryId });

            modelBuilder.Entity<CouponCategory>()
                .HasOne(cc => cc.Coupon)
                .WithMany(c => c.CouponCategories)
                .HasForeignKey(cc => cc.CouponId);

            modelBuilder.Entity<CouponCategory>()
                .HasOne(cc => cc.Category)
                .WithMany()
                .HasForeignKey(cc => cc.CategoryId);


            // ============================================================
            // CouponUsage
            // ============================================================

            modelBuilder.Entity<CouponUsage>()
                .HasOne(u => u.Coupon)
                .WithMany(c => c.Usages)
                .HasForeignKey(u => u.CouponId);

            modelBuilder.Entity<CouponUsage>()
                .HasOne(u => u.User)
                .WithMany()
                .HasForeignKey(u => u.UserId);

            modelBuilder.Entity<CouponUsage>()
                .HasOne(u => u.Order)
                .WithMany()
                .HasForeignKey(u => u.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CouponUsage>()
                .Property(u => u.DiscountAmountApplied)
                .HasPrecision(18, 2);


            // ============================================================
            // Cart coupon fields
            // ============================================================

            modelBuilder.Entity<Cart>()
                .Property(c => c.DiscountAmount)
                .HasPrecision(18, 2);


            // ============================================================
            // Order coupon fields
            // ============================================================

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);


            // ============================================================
            // CampaignProduct
            // ============================================================

            modelBuilder.Entity<CampaignProduct>()
                .HasKey(cp => new { cp.CampaignId, cp.ProductId });

            modelBuilder.Entity<CampaignProduct>()
                .HasOne(cp => cp.Campaign)
                .WithMany(c => c.CampaignProducts)
                .HasForeignKey(cp => cp.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CampaignProduct>()
                .HasOne(cp => cp.Product)
                .WithMany()
                .HasForeignKey(cp => cp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================================
            // CampaignCategory
            // ============================================================

            modelBuilder.Entity<CampaignCategory>()
                .HasKey(cc => new { cc.CampaignId, cc.CategoryId });

            modelBuilder.Entity<CampaignCategory>()
                .HasOne(cc => cc.Campaign)
                .WithMany(c => c.CampaignCategories)
                .HasForeignKey(cc => cc.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CampaignCategory>()
                .HasOne(cc => cc.Category)
                .WithMany()
                .HasForeignKey(cc => cc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================================
            // Payment → Order
            // ============================================================

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.RefundedAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.PaymentIntentId);


            // ============================================================
            // RefundRequest → Order/User
            // ============================================================

            modelBuilder.Entity<RefundRequest>()
                .HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefundRequest>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================================
            // Address → User
            // ============================================================

            modelBuilder.Entity<Address>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================================
            // Order → SavedAddress
            // ============================================================

            modelBuilder.Entity<Order>()
                .HasOne(o => o.SavedAddress)
                .WithMany()
                .HasForeignKey(o => o.AddressId)
                .OnDelete(DeleteBehavior.SetNull);


            // ============================================================
            // Order → DeliveryMethod
            // ============================================================

            modelBuilder.Entity<Order>()
                .HasOne(o => o.DeliveryMethod)
                .WithMany()
                .HasForeignKey(o => o.DeliveryMethodId)
                .OnDelete(DeleteBehavior.SetNull);


            // ============================================================
            // Order → Shipment (1:1)
            // ============================================================

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Shipment)
                .WithOne(s => s.Order)
                .HasForeignKey<Shipment>(s => s.OrderId)
                .OnDelete(DeleteBehavior.Cascade);


            // ============================================================
            // Shipment → ShipmentStatusHistory
            // ============================================================

            modelBuilder.Entity<Shipment>()
                .HasMany(s => s.StatusHistory)
                .WithOne(h => h.Shipment)
                .HasForeignKey(h => h.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);


            // Decimal precision for shipping
            modelBuilder.Entity<DeliveryMethod>()
                .Property(d => d.Price)
                .HasPrecision(18, 2);


            // ============================================================
            // Seed default delivery methods
            // ============================================================
            //
            // PublicId values are fixed because HasData requires
            // deterministic seed data.
            // ============================================================

            modelBuilder.Entity<DeliveryMethod>().HasData(
                new DeliveryMethod
                {
                    Id = 1,
                    PublicId = Guid.Parse("7f9c8e31-4a72-4b1f-8c36-1a6e9b7d2401"),
                    Name = "Standard",
                    Description = "5–7 business days",
                    Price = 4.99m,
                    EstimatedDaysMin = 5,
                    EstimatedDaysMax = 7,
                    IsActive = true,
                    SortOrder = 1
                },
                new DeliveryMethod
                {
                    Id = 2,
                    PublicId = Guid.Parse("c4e2a951-8d63-47b9-a215-6f3c8b742e10"),
                    Name = "Express",
                    Description = "1–2 business days",
                    Price = 14.99m,
                    EstimatedDaysMin = 1,
                    EstimatedDaysMax = 2,
                    IsActive = true,
                    SortOrder = 2
                }
            );


            // ============================================================
            // Refresh Tokens
            // ============================================================

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasOne(rt => rt.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(rt => rt.TokenHash)
                    .IsUnique();
            });


            // ============================================================
            // AuditLog
            // ============================================================

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(a => a.CreatedAt);
                entity.HasIndex(a => a.UserId);
                entity.HasIndex(a => new { a.EntityName, a.EntityId });

                entity.Property(a => a.Action)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(a => a.EntityName)
                    .HasMaxLength(100);

                entity.Property(a => a.EntityId)
                    .HasMaxLength(100);

                entity.Property(a => a.UserEmail)
                    .HasMaxLength(256);

                entity.Property(a => a.IpAddress)
                    .HasMaxLength(64);

                entity.Property(a => a.Details)
                    .HasMaxLength(1000);
            });
        }
    }
}