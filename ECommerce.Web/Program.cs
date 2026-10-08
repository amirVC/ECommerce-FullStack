using ECommerce.Web.Interfaces;
using ECommerce.Web.Options;
using ECommerce.Web.Services;
using ECommerce.Web.Services.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using ECommerce.Web.Middleware;

namespace ECommerce.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddTransient<TokenRefreshHandler>();

            builder.Services.AddHttpClient("ApiClient", (serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(
                    configuration["ApiSettings:BaseUrl"]!);
            })
            .AddHttpMessageHandler<TokenRefreshHandler>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddMemoryCache();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IApiClient, ApiClient>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IAdminCategoryService, AdminCategoryService>();
            builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
            builder.Services.AddScoped<IAdminUserService, AdminUserService>();
            builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            builder.Services.AddScoped<IWishlistService, WishlistService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IAdminReviewService, AdminReviewService>();
            builder.Services.AddScoped<IRecentlyViewedService, RecentlyViewedService>();
            builder.Services.AddScoped<IAdminCouponService, AdminCouponService>();
            builder.Services.Configure<ShippingOptions>(builder.Configuration.GetSection("ShippingSettings"));
            builder.Services.AddScoped<IAdminCampaignService, AdminCampaignService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection(StripeOptions.SectionName));
            builder.Services.AddScoped<IRefundRequestService, RefundRequestService>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<IDeliveryMethodService, DeliveryMethodService>();
            builder.Services.AddScoped<IShipmentService, ShipmentService>();
            builder.Services.AddScoped<IAdminAuditLogService, AdminAuditLogService>();

            // ==========================
            // Response Compression
            // ==========================
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            });

            builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(o =>
                o.Level = System.IO.Compression.CompressionLevel.Fastest);

            builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(o =>
                o.Level = System.IO.Compression.CompressionLevel.Fastest);

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                    options.ExpireTimeSpan = TimeSpan.FromHours(2);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IAdminProductService, AdminProductService>();
            builder.Services.AddScoped<ICartService, CartService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseResponseCompression();
            app.UseMiddleware<PrecompressedStaticFileMiddleware>();

            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.Run();
        }
    }
}