using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ECommerce.Web.Exceptions;

namespace ECommerce.Web.Services.Http;

public class ApiClient : IApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpClient CreateClient(string? token = null)
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        // If no token was explicitly supplied, get it directly from Session.
        token ??= _httpContextAccessor.HttpContext?
            .Session.GetString("JWToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private async Task ThrowApiException(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();

        string message = "An unexpected error occurred.";

        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                using var document = JsonDocument.Parse(json);

                if (document.RootElement.TryGetProperty(
                    "Message",
                    out var messageProperty))
                {
                    message = messageProperty.GetString() ?? message;
                }
                else if (document.RootElement.TryGetProperty(
                    "message",
                    out messageProperty))
                {
                    message = messageProperty.GetString() ?? message;
                }
                else
                {
                    message = json;
                }
            }
            catch
            {
                message = json;
            }
        }

        throw new ApiException(
            message,
            (int)response.StatusCode);
    }

    public async Task<T?> GetAsync<T>(
        string url,
        string? token = null)
    {
        var client = CreateClient(token);

        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            await ThrowApiException(response);

        var json = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(
            json,
            _jsonOptions);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string url,
        TRequest body,
        string? token = null)
    {
        var client = CreateClient(token);

        var json = JsonSerializer.Serialize(body);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
            await ThrowApiException(response);

        var responseJson =
            await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseJson))
            return default;

        return JsonSerializer.Deserialize<TResponse>(
            responseJson,
            _jsonOptions);
    }

    public async Task<bool> PostAsync<TRequest>(
        string url,
        TRequest body,
        string? token = null)
    {
        var client = CreateClient(token);

        var json = JsonSerializer.Serialize(body);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
            await ThrowApiException(response);

        return true;
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string url,
        TRequest body,
        string? token = null)
    {
        var client = CreateClient(token);

        var json = JsonSerializer.Serialize(body);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync(url, content);

        if (!response.IsSuccessStatusCode)
            await ThrowApiException(response);

        var responseJson =
            await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseJson))
            return default;

        return JsonSerializer.Deserialize<TResponse>(
            responseJson,
            _jsonOptions);
    }

    public async Task<TResponse?> DeleteAsync<TResponse>(
        string url,
        string? token = null)
    {
        var client = CreateClient(token);

        var response = await client.DeleteAsync(url);

        if (!response.IsSuccessStatusCode)
            await ThrowApiException(response);

        var responseJson =
            await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseJson))
            return default;

        return JsonSerializer.Deserialize<TResponse>(
            responseJson,
            _jsonOptions);
    }

    public async Task<TResponse?> PostMultipartAsync<TResponse>(
        string url,
        MultipartFormDataContent content,
        string? token = null)
    {
        var client = CreateClient(token);

        var response = await client.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
            await ThrowApiException(response);

        var responseJson =
            await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseJson))
            return default;

        return JsonSerializer.Deserialize<TResponse>(
            responseJson,
            _jsonOptions);
    }

    public async Task<TResponse?> PutMultipartAsync<TResponse>(
        string url,
        MultipartFormDataContent content,
        string? token = null)
    {
        var client = CreateClient(token);

        var response = await client.PutAsync(url, content);

        if (!response.IsSuccessStatusCode)
            await ThrowApiException(response);

        var responseJson =
            await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseJson))
            return default;

        return JsonSerializer.Deserialize<TResponse>(
            responseJson,
            _jsonOptions);
    }
}