using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace ECommerce.Web.Services.Base;
public abstract class ApiServiceBase
{
    protected readonly IHttpClientFactory HttpClientFactory;
    protected readonly IHttpContextAccessor HttpContextAccessor;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    protected ApiServiceBase(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        HttpClientFactory = httpClientFactory;
        HttpContextAccessor = httpContextAccessor;
    }
    protected HttpClient CreateClient()
    {
        var client = HttpClientFactory.CreateClient("ApiClient");
        var token = HttpContextAccessor.HttpContext?
            .Session
            .GetString("JWToken");
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }
    protected async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }
    protected async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        return await response.Content.ReadAsStringAsync();
    }
}