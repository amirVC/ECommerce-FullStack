using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ECommerce.Web.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ECommerce.Web.Services.Http;

// Registered as a message handler on the "ApiClient" named HttpClient
// (see Program.cs). Every request that comes back 401 gets one retry:
// the refresh token in session is exchanged for a new access token,
// which is then used to resend the original request.
//
// Refresh/login calls themselves are excluded from this logic so a bad
// refresh token can't trigger a refresh loop.
public class TokenRefreshHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;

    // Guards against multiple concurrent requests all trying to refresh
    // the same session's token at once.
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string RetryFlagHeader = "X-Retried-After-Refresh";

    public TokenRefreshHandler(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        if (IsAuthEndpoint(request) || request.Headers.Contains(RetryFlagHeader))
            return response;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return response;

        var refreshToken = httpContext.Session.GetString("RefreshToken");
        if (string.IsNullOrWhiteSpace(refreshToken))
            return response;

        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            // Another request may have refreshed while we were waiting.
            // Re-read from session before deciding whether to call the API.
            var newAccessToken = await TryRefreshAsync(httpContext, refreshToken, cancellationToken);

            if (newAccessToken == null)
            {
                // Refresh token is no longer valid; clear local auth state
                // so the app doesn't think it's still logged in.
                httpContext.Session.Remove("JWToken");
                httpContext.Session.Remove("RefreshToken");
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return response;
            }

            var retryRequest = await CloneRequestAsync(request);
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);
            retryRequest.Headers.TryAddWithoutValidation(RetryFlagHeader, "1");

            response.Dispose();
            return await base.SendAsync(retryRequest, cancellationToken);
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private async Task<string?> TryRefreshAsync(
        HttpContext httpContext,
        string refreshToken,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        var body = JsonSerializer.Serialize(new RefreshTokenRequestDto { RefreshToken = refreshToken });
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh") { Content = content };
        refreshRequest.Headers.TryAddWithoutValidation(RetryFlagHeader, "1");

        HttpResponseMessage refreshResponse;
        try
        {
            refreshResponse = await client.SendAsync(refreshRequest, cancellationToken);
        }
        catch
        {
            return null;
        }

        if (!refreshResponse.IsSuccessStatusCode)
            return null;

        var json = await refreshResponse.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<LoginResponseDto>(json, JsonOptions);

        if (result?.AccessToken == null || result.RefreshToken == null)
            return null;

        httpContext.Session.SetString("JWToken", result.AccessToken);
        httpContext.Session.SetString("RefreshToken", result.RefreshToken);

        return result.AccessToken;
    }

    private static bool IsAuthEndpoint(HttpRequestMessage request)
    {
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;

        return path.Contains("/api/auth/refresh", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/api/auth/login", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/api/auth/register", StringComparison.OrdinalIgnoreCase);
    }

    // A sent HttpRequestMessage can't be reused, so build an equivalent one.
    // Content is reused by reference rather than re-serialized: this works
    // for StringContent/JsonContent bodies (the common case here) since
    // their underlying buffer can be re-read. Streamed multipart uploads
    // (file/image uploads) are a known edge case that won't survive a
    // retry if their stream was already fully consumed.
    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Content = request.Content,
            Version = request.Version
        };

        foreach (var header in request.Headers)
        {
            if (header.Key == HttpRequestHeader.Authorization.ToString())
                continue;

            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in request.Options)
        {
            clone.Options.TryAdd(option.Key, option.Value);
        }

        await Task.CompletedTask;
        return clone;
    }
}