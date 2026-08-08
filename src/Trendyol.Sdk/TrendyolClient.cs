using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Trendyol.Sdk.Catalog;
using Trendyol.Sdk.Configuration;
using Trendyol.Sdk.Inventory;
using Trendyol.Sdk.Internal.Http;
using Trendyol.Sdk.Invoices;
using Trendyol.Sdk.Orders;
using Trendyol.Sdk.Products;
using Trendyol.Sdk.Questions;
using Trendyol.Sdk.Returns;
using Trendyol.Sdk.Webhooks;

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
        SellerId = snapshot.SellerId;
        Catalog = new CatalogClient(this);
        Products = new ProductsClient(this);
        Inventory = new InventoryClient(this);
        Orders = new OrdersClient(this);
        Returns = new ReturnsClient(this);
        Questions = new QuestionsClient(this);
        Invoices = new InvoicesClient(this);
        Webhooks = new WebhooksClient(this);
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
        SellerId = options.SellerId;
        Catalog = new CatalogClient(this);
        Products = new ProductsClient(this);
        Inventory = new InventoryClient(this);
        Orders = new OrdersClient(this);
        Returns = new ReturnsClient(this);
        Questions = new QuestionsClient(this);
        Invoices = new InvoicesClient(this);
        Webhooks = new WebhooksClient(this);
    }

    /// <summary>
    /// Gets the Trendyol catalog operations implemented by this SDK.
    /// </summary>
    public ICatalogClient Catalog { get; }

    /// <summary>Gets Product API V2 operations.</summary>
    public IProductsClient Products { get; }

    /// <summary>Gets inventory and price operations.</summary>
    public IInventoryClient Inventory { get; }

    /// <summary>Gets order and shipment-package operations.</summary>
    public IOrdersClient Orders { get; }

    /// <summary>Gets return and claim operations.</summary>
    public IReturnsClient Returns { get; }

    /// <summary>Gets customer-question operations.</summary>
    public IQuestionsClient Questions { get; }

    /// <summary>Gets seller-invoice operations.</summary>
    public IInvoicesClient Invoices { get; }

    /// <summary>Gets webhook operations.</summary>
    public IWebhooksClient Webhooks { get; }

    internal long SellerId { get; }

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

    internal Task SendAsync(
        string operation,
        HttpMethod method,
        string relativeUri,
        string routeTemplate,
        object? requestBody,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return _transport.SendAsync(operation, method, relativeUri, routeTemplate, requestBody, cancellationToken);
    }

    internal Task<TResponse?> SendContentAsync<TResponse>(
        string operation,
        HttpMethod method,
        string relativeUri,
        string routeTemplate,
        HttpContent content,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return _transport.SendContentAsync<TResponse>(
            operation, method, relativeUri, routeTemplate, content, cancellationToken);
    }

    internal Task SendContentAsync(
        string operation,
        HttpMethod method,
        string relativeUri,
        string routeTemplate,
        HttpContent content,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return _transport.SendContentAsync(operation, method, relativeUri, routeTemplate, content, cancellationToken);
    }

    private void ThrowIfDisposed()
    {
#if NET10_0_OR_GREATER
        ObjectDisposedException.ThrowIf(_disposed, this);
#else
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TrendyolClient));
        }
#endif
    }

    private static HttpClient CreateHttpClient(TrendyolOptionsSnapshot options) => new()
    {
        BaseAddress = options.BaseAddress,
        Timeout = options.Timeout,
    };
}
