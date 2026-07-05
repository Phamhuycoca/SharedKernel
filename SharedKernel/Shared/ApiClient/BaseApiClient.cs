using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Common.Wrappers.ErrorResponse;
using SharedKernel.Shared.ApiClient;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SharedKernel.Shared;

public class BaseApiClient : IBaseApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ServiceEndpointOptions _options;
    private readonly ILogger<BaseApiClient> _logger;

    public BaseApiClient(
        HttpClient httpClient,
        IOptions<ServiceEndpointOptions> options,
        ILogger<BaseApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(
        string serviceName,
        string url,
        string? bearerToken = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, serviceName, url, bearerToken, headers);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await HandleResponseAsync<T>(response, cancellationToken);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string serviceName,
        string url,
        TRequest requestBody,
        string? bearerToken = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, serviceName, url, bearerToken, headers);
        request.Content = JsonContent.Create(requestBody);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await HandleResponseAsync<TResponse>(response, cancellationToken);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string serviceName,
        string url,
        TRequest requestBody,
        string? bearerToken = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Put, serviceName, url, bearerToken, headers);
        request.Content = JsonContent.Create(requestBody);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return await HandleResponseAsync<TResponse>(response, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        string serviceName,
        string url,
        string? bearerToken = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Delete, serviceName, url, bearerToken, headers);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            await ThrowApiExceptionAsync(response, cancellationToken);

        return true;
    }

    private HttpRequestMessage CreateRequest(
        HttpMethod method,
        string serviceName,
        string url,
        string? bearerToken,
        Dictionary<string, string>? headers)
    {
        if (!_options.Services.TryGetValue(serviceName, out var baseUrl))
            throw new AppException($"Service '{serviceName}' not found");

        var request = new HttpRequestMessage(method, $"{baseUrl}{url}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        if (headers is not null)
        {
            foreach (var (key, value) in headers)
                request.Headers.TryAddWithoutValidation(key, value);
        }

        return request;
    }

    private async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiExceptionAsync(response, cancellationToken);
        }

        if (response.Content.Headers.ContentLength == 0)
            return default;

        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        return result;
    }

    private async Task ThrowApiExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogError(
            "API call failed with status {StatusCode}. Response: {Content}",
            response.StatusCode, content);

        var message = content;

        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            if (json.TryGetProperty("message", out var msg))
                message = msg.GetString() ?? content;
        }
        catch (JsonException)
        {
            // Body không phải JSON hợp lệ -> giữ nguyên content làm message
        }

        throw new AppException(response.StatusCode, message);
    }
}