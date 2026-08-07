using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Trendyol.Sdk.Catalog;
using Trendyol.Sdk.Configuration;
using Trendyol.Sdk.Internal.Http;

namespace Trendyol.Sdk;

/// <summary>
/// Provides access to implemented Trendyol Türkiye Marketplace API feature clients.
/// </summary>
/// <remarks>
/// Dispose directly constructed clients when they are no longer needed. Clients resolved from dependency injection are disposed by the container.
/// </remarks>
public sealed class TrendyolClient : IDisposable
{
    private readonly bool _disposeHttpClient;
    private readonly HttpClient _httpClient;
    private readonly TrendyolHttpTransport _transport;
    private bool _disposed;

    /// <summary>
    /// Initializes a client with one internally owned <see cref="HttpClient" />.
    /// </summary>
    /// <param name="options">The validated Trendyol configuration.</param>
    public TrendyolClient(TrendyolOptions options)
    {
        var snapshot = TrendyolOptionsSnapshot.Create(options);
        _httpClient = CreateHttpClient(snapshot);
        _disposeHttpClient = true;
        _transport = new TrendyolHttpTransport(_httpClient, snapshot, NullLogger.Instance);
        Catalog = new CatalogClient(this);
    }

    internal TrendyolClient(
        HttpClient httpClient,
        TrendyolOptionsSnapshot options,
        ILogger logger,
        bool disposeHttpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _disposeHttpClient = disposeHttpClient;
        _transport = new TrendyolHttpTransport(httpClient, options, logger);
        Catalog = new CatalogClient(this);
    }

    /// <summary>
    /// Gets the Trendyol catalog operations implemented by this SDK.
    /// </summary>
    public ICatalogClient Catalog { get; }

    /// <summary>
    /// Releases resources owned by this facade.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_disposeHttpClient)
        {
            _httpClient.Dispose();
        }

        _disposed = true;
    }

    internal Task<TResponse?> SendAsync<TResponse>(
        string operation,
        HttpMethod method,
        string relativeUri,
        string routeTemplate,
        object? requestBody,
        CancellationToken cancellationToken)
    {
#if NET10_0_OR_GREATER
        ObjectDisposedException.ThrowIf(_disposed, this);
#else
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TrendyolClient));
        }
#endif

        return _transport.SendAsync<TResponse>(
            operation,
            method,
            relativeUri,
            routeTemplate,
            requestBody,
            cancellationToken);
    }

    private static HttpClient CreateHttpClient(TrendyolOptionsSnapshot options) => new()
    {
        BaseAddress = options.BaseAddress,
        Timeout = options.Timeout,
    };
}
