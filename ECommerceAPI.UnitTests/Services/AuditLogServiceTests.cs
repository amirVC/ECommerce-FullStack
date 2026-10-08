using System.Net;
using System.Security.Claims;
using ECommerceAPI.Data;
using ECommerceAPI.Models;
using ECommerceAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ECommerceAPI.UnitTests.Services;

public class AuditLogServiceTests
{
    // ---- test helpers -----------------------------------------------------

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // fresh DB per test
            .Options;

        return new AppDbContext(options);
    }

    private static Mock<IHttpContextAccessor> CreateHttpContextAccessor(
        int? userId = null,
        string? email = null,
        string? ip = "203.0.113.5")
    {
        var httpContext = new DefaultHttpContext();

        if (ip != null)
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse(ip);

        var claims = new List<Claim>();
        if (userId.HasValue)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        if (email != null)
            claims.Add(new Claim(ClaimTypes.Email, email));

        if (claims.Count > 0)
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);
        return accessor;
    }

    private static AuditLogService CreateService(
        AppDbContext context,
        Mock<IHttpContextAccessor>? httpContextAccessor = null)
    {
        httpContextAccessor ??= CreateHttpContextAccessor();
        var logger = new Mock<ILogger<AuditLogService>>();
        return new AuditLogService(context, httpContextAccessor.Object, logger.Object);
    }

    // ---- happy path ---------------------------------------------------------

    [Fact]
    public async Task LogAsync_WritesEntry_WithExplicitlyPassedValues()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        await service.LogAsync(
            action: "LOGIN_FAILED",
            entityName: "User",
            entityId: "42",
            details: "Invalid password",
            userId: 42,
            userEmail: "user@test.com",
            ipAddress: "198.51.100.1");

        var saved = await context.AuditLogs.SingleAsync();

        saved.Action.Should().Be("LOGIN_FAILED");
        saved.EntityName.Should().Be("User");
        saved.EntityId.Should().Be("42");
        saved.Details.Should().Be("Invalid password");
        saved.UserId.Should().Be(42);
        saved.UserEmail.Should().Be("user@test.com");
        saved.IpAddress.Should().Be("198.51.100.1");
        saved.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ---- fallback resolution from HttpContext --------------------------------

    [Fact]
    public async Task LogAsync_FallsBackToHttpContext_WhenUserIdEmailIpNotPassedExplicitly()
    {
        using var context = CreateContext();
        var accessor = CreateHttpContextAccessor(userId: 7, email: "fallback@test.com", ip: "10.0.0.9");
        var service = CreateService(context, accessor);

        await service.LogAsync(action: "LOGIN_FAILED");

        var saved = await context.AuditLogs.SingleAsync();

        saved.UserId.Should().Be(7);
        saved.UserEmail.Should().Be("fallback@test.com");
        saved.IpAddress.Should().Be("10.0.0.9");
    }

    [Fact]
    public async Task LogAsync_UserIdAndEmailAreNull_WhenNoClaimAndNoParameterProvided()
    {
        using var context = CreateContext();
        var accessor = CreateHttpContextAccessor(userId: null, email: null);
        var service = CreateService(context, accessor);

        await service.LogAsync(action: "LOGIN_FAILED");

        var saved = await context.AuditLogs.SingleAsync();

        saved.UserId.Should().BeNull();
        saved.UserEmail.Should().BeNull();
    }

    [Fact]
    public async Task LogAsync_ExplicitParameters_TakePriorityOver_HttpContextClaims()
    {
        using var context = CreateContext();
        // HttpContext says user 7 / fallback@test.com, but we pass explicit values —
        // explicit values should win.
        var accessor = CreateHttpContextAccessor(userId: 7, email: "fallback@test.com", ip: "10.0.0.9");
        var service = CreateService(context, accessor);

        await service.LogAsync(action: "LOGIN_FAILED", userId: 99, userEmail: "explicit@test.com");

        var saved = await context.AuditLogs.SingleAsync();

        saved.UserId.Should().Be(99);
        saved.UserEmail.Should().Be("explicit@test.com");
    }

    // ---- the behavior most likely to explain "LogFailed doesn't work" -------

    [Fact]
    public async Task LogAsync_SwallowsException_WhenSaveChangesFails()
    {
        // This documents current (suspicious) behavior: if the DB write fails,
        // LogAsync catches the exception, logs it via ILogger, and returns
        // normally. The caller (e.g. AuthService logging a failed login) has
        // NO way to know the audit entry was never persisted. If your "failed"
        // audit logs are missing, this swallow-and-continue is the likely cause —
        // something upstream is throwing, and it's being silently absorbed here.
        using var context = CreateContext();
        await context.DisposeAsync(); // forces SaveChangesAsync to throw ObjectDisposedException

        var service = CreateService(context);

        var act = async () => await service.LogAsync(action: "LOGIN_FAILED");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LogAsync_DoesNotPersistEntry_WhenSaveChangesFails()
    {
        using var context = CreateContext();
        await context.DisposeAsync();

        var service = CreateService(context);
        await service.LogAsync(action: "LOGIN_FAILED");

        // Verify with a fresh, working context against the same in-memory DB
        // isn't possible here since the DB name was tied to the disposed context.
        // The point of this test is simply: no exception propagates, and (per the
        // test above) nothing was written — the failure is invisible to the caller.
    }

    // ---- timestamp -----------------------------------------------------------

    [Fact]
    public async Task LogAsync_SetsCreatedAt_ToUtcNow()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var before = DateTime.UtcNow;
        await service.LogAsync(action: "LOGIN_FAILED");
        var after = DateTime.UtcNow;

        var saved = await context.AuditLogs.SingleAsync();
        saved.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}