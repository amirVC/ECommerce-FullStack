using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using ECommerceAPI.Data;
using ECommerceAPI.Middleware;
using ECommerceAPI.Options;
using ECommerceAPI.Services;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// Serilog
// ==========================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ECommerceAPI")
    .CreateLogger();

builder.Host.UseSerilog();

// ==========================
// Controllers + JSON Options
// ==========================
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;

        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// ==========================
// Caching
// ==========================
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// ==========================
// Swagger + JWT
// ==========================
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer {your_token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ==========================
// Database
// ==========================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// ==========================
// Health Checks
// ==========================
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql",
        tags: new[] { "database", "postgresql" });

// ==========================
// Services
// ==========================
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<IAdminProductService, AdminProductService>();
builder.Services.AddScoped<IAdminCategoryService, AdminCategoryService>();
builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IProductImageService, ProductImageService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAdminReviewService, AdminReviewService>();
builder.Services.AddScoped<IRecentlyViewedService, RecentlyViewedService>();

builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();

builder.Services.Configure<StripeOptions>(
    builder.Configuration.GetSection(StripeOptions.SectionName));

builder.Services.AddScoped<IPaymentService, StripePaymentService>();

builder.Services.AddScoped<IRefundRequestService, RefundRequestService>();
builder.Services.AddScoped<IAdminPaymentService, AdminPaymentService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IDeliveryMethodService, DeliveryMethodService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<GmailSmtpOptions>(
    builder.Configuration.GetSection("GmailSmtp"));

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.Configure<ImageOptimizationOptions>(
    builder.Configuration.GetSection("ImageOptimization"));

// ==========================
// Authentication
// ==========================
builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!
                )
            )
        };
    });

// ==========================
// Authorization
// ==========================
builder.Services.AddAuthorization();

// ==========================
// Rate Limiting
// ==========================
builder.Services.AddRateLimiter(options =>
{
    // HTTP 429 when the rate limit is exceeded
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    // ==========================================
    // GLOBAL RATE LIMIT
    // ==========================================
    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        httpContext.Connection.RemoteIpAddress
                            ?.ToString()
                        ?? "unknown",

                    factory: _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

    // ==========================================
    // AUTH RATE LIMIT
    // ==========================================
    options.AddPolicy(
        "auth",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    httpContext.Connection.RemoteIpAddress
                        ?.ToString()
                    ?? "unknown",

                factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

    // ==========================================
    // PASSWORD RESET RATE LIMIT
    // ==========================================
    options.AddPolicy(
        "password-reset",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    httpContext.Connection.RemoteIpAddress
                        ?.ToString()
                    ?? "unknown",

                factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(15),
                        QueueLimit = 0
                    }));

    // ==========================================
    // EMAIL OTP RATE LIMIT
    // ==========================================
    options.AddPolicy(
        "otp",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    httpContext.Connection.RemoteIpAddress
                        ?.ToString()
                    ?? "unknown",

                factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0
                    }));

    // ==========================================
    // PAYMENT RATE LIMIT
    // ==========================================
    options.AddPolicy(
        "payment",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    httpContext.Connection.RemoteIpAddress
                        ?.ToString()
                    ?? "unknown",

                factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

    // ==========================================
    // CART RATE LIMIT
    // ==========================================
    options.AddPolicy(
        "cart",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    httpContext.Connection.RemoteIpAddress
                        ?.ToString()
                    ?? "unknown",

                factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

    // ==========================================
    // ORDER CREATION RATE LIMIT
    // ==========================================
    options.AddPolicy(
        "order",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    httpContext.Connection.RemoteIpAddress
                        ?.ToString()
                    ?? "unknown",

                factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

    // ==========================================
    // REVIEW RATE LIMIT
    // ==========================================
    options.AddPolicy(
        "review",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    httpContext.Connection.RemoteIpAddress
                        ?.ToString()
                    ?? "unknown",

                factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0
                    }));
});

// ==========================
// Response Compression
// ==========================
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;

    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();

    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();

    options.MimeTypes =
        Microsoft.AspNetCore.ResponseCompression
            .ResponseCompressionDefaults
            .MimeTypes
            .Concat(new[]
            {
                "application/json",
                "application/json; charset=utf-8"
            });
});

builder.Services.Configure<
    Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(
    options =>
    {
        options.Level =
            System.IO.Compression.CompressionLevel.Fastest;
    });

builder.Services.Configure<
    Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(
    options =>
    {
        options.Level =
            System.IO.Compression.CompressionLevel.Fastest;
    });

// ==========================
// AutoMapper
// ==========================
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// ==========================
// SEED DATABASE
// ==========================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    await SeedData.Initialize(context);
}

// ==========================
// Swagger
// ==========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ==========================
// Middleware
// ==========================
app.UseHttpsRedirection();

app.UseResponseCompression();

app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionMiddleware>();

app.UseStaticFiles();

app.UseRouting();

app.UseResponseCaching();

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

// ==========================
// Health Checks
// ==========================
app.MapHealthChecks("/health");

// ==========================
// Controllers
// ==========================
app.MapControllers();

app.Run();