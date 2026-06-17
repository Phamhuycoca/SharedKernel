using Microsoft.Extensions.Options;
using SharedKernel.Common.Wrapper.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharedKernel.Shared;

public class BaseApiClient : IBaseApiClient
{
    private readonly HttpClient _httpClient;

    private readonly ServiceEndpointOptions _options;

    public BaseApiClient(
        HttpClient httpClient,
        IOptions<ServiceEndpointOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<T?> GetAsync<T>(
        string serviceName,
        string url,
        string? bearerToken = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(
            HttpMethod.Get,
            serviceName,
            url,
            bearerToken,
            headers);

        var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        return await HandleResponse<T>(response);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string serviceName,
        string url,
        TRequest requestBody,
        string? bearerToken = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(
            HttpMethod.Post,
            serviceName,
            url,
            bearerToken,
            headers);

        request.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        return await HandleResponse<TResponse>(response);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string serviceName,
        string url,
        TRequest requestBody,
        string? bearerToken = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(
            HttpMethod.Put,
            serviceName,
            url,
            bearerToken,
            headers);

        request.Content = JsonContent.Create(requestBody);

        var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        return await HandleResponse<TResponse>(response);
    }

    public async Task<bool> DeleteAsync(
        string serviceName,
        string url,
        string? bearerToken = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(
            HttpMethod.Delete,
            serviceName,
            url,
            bearerToken,
            headers);

        var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowApiException(response);
        }

        return true;
    }

    private HttpRequestMessage CreateRequest(
        HttpMethod method,
        string serviceName,
        string url,
        string? bearerToken,
        Dictionary<string, string>? headers)
    {
        if (!_options.Services.TryGetValue(
            serviceName,
            out var baseUrl))
        {
            throw new AppException(
                $"Service '{serviceName}' not found");
        }

        var request = new HttpRequestMessage(
            method,
            $"{baseUrl}{url}");

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                "application/json"));

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    bearerToken);
        }

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Remove(header.Key);

                request.Headers.Add(
                    header.Key,
                    header.Value);
            }
        }

        return request;
    }
    private async Task<T?> HandleResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[ERROR] {response.StatusCode}");
            Console.WriteLine(content);

            await ThrowApiException(response);
        }

        if (string.IsNullOrWhiteSpace(content))
            return default;

        return JsonSerializer.Deserialize<T>(
            content,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

    private async Task ThrowApiException(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        string message = "Unknown error";
        object? errorObj = null;

        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(content);

            if (json.TryGetProperty("message", out var msg))
            {
                message = msg.GetString() ?? message;
            }

            errorObj = json;
        }
        catch
        {
            message = content;
            errorObj = content;
        }

        throw new AppException(
            response.StatusCode,
            message);
    }
}
