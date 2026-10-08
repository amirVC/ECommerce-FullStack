using System.Net.Http.Headers;

namespace ECommerce.Web.Services.Http;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string url, string? token = null);

    Task<TResponse?> PostAsync<TRequest, TResponse>(
        string url, TRequest body, string? token = null);

    Task<bool> PostAsync<TRequest>(
        string url, TRequest body, string? token = null);

    Task<TResponse?> PutAsync<TRequest, TResponse>(
        string url, TRequest body, string? token = null);

    Task<TResponse?> DeleteAsync<TResponse>(
        string url, string? token = null);

    Task<TResponse?> PostMultipartAsync<TResponse>(
        string url, MultipartFormDataContent content, string? token = null);

    Task<TResponse?> PutMultipartAsync<TResponse>(       // ADD THIS
        string url, MultipartFormDataContent content, string? token = null);
}