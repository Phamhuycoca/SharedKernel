using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SharedKernel.Shared.ApiClient;

public interface IBaseApiClient
{
    Task<T?> GetAsync<T>(
        string serviceName,
        string url,
        string? bearerToken = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    Task<TResponse?> PostAsync<TRequest, TResponse>(
        string serviceName,
        string url,
        TRequest request,
        string? bearerToken = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    Task<TResponse?> PutAsync<TRequest, TResponse>(
        string serviceName,
        string url,
        TRequest request,
        string? bearerToken = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string serviceName,
        string url,
        string? bearerToken = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default);
}