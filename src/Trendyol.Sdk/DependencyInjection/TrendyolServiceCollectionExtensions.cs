using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Trendyol.Sdk;
using Trendyol.Sdk.Catalog;
using Trendyol.Sdk.Configuration;
using Trendyol.Sdk.Inventory;
using Trendyol.Sdk.Invoices;
using Trendyol.Sdk.Orders;
using Trendyol.Sdk.Products;
using Trendyol.Sdk.Questions;
using Trendyol.Sdk.Returns;
using Trendyol.Sdk.Webhooks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides dependency-injection registration for Trendyol.Sdk.
/// </summary>
public static class TrendyolServiceCollectionExtensions
{
    /// <summary>
    /// Registers a factory-managed <see cref="TrendyolClient" /> for the Trendyol Türkiye Marketplace API.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">The callback used to configure API credentials and client behavior.</param>
    /// <returns>The HTTP client builder, which can be used to add deliberate application-specific handlers.</returns>
    public static IHttpClientBuilder AddTrendyol(
        this IServiceCollection services,
        Action<TrendyolOptions> configureOptions)
    {
#if NET10_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
#else
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configureOptions is null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }
#endif

        services
            .AddOptions<TrendyolOptions>()
            .Configure(configureOptions)
            .Validate(static options => options.SellerId > 0, "SellerId must be greater than zero.")
            .Validate(static options => !string.IsNullOrWhiteSpace(options.ApiKey), "ApiKey must not be empty.")
            .Validate(static options => !string.IsNullOrWhiteSpace(options.ApiSecret), "ApiSecret must not be empty.")
            .Validate(
                static options => TrendyolOptionsSnapshot.IsValidIntegratorName(options.IntegratorName),
                "IntegratorName must contain only ASCII letters and digits and must be between 1 and 30 characters.")
            .Validate(
                static options => TrendyolOptionsSnapshot.IsDefinedEnvironment(options.Environment),
                "Environment must be Production or Stage.")
            .Validate(static options => options.Timeout > TimeSpan.Zero, "Timeout must be greater than zero.");

        services.TryAddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        var builder = services.AddHttpClient(nameof(TrendyolClient));
        builder.AddTypedClient(static (httpClient, serviceProvider) =>
        {
            var configuredOptions = serviceProvider
                .GetRequiredService<IOptions<TrendyolOptions>>()
                .Value;
            var snapshot = TrendyolOptionsSnapshot.Create(configuredOptions);
            httpClient.BaseAddress = snapshot.BaseAddress;
            httpClient.Timeout = snapshot.Timeout;

            var logger = serviceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger<TrendyolClient>();

            return new TrendyolClient(httpClient, snapshot, logger, disposeHttpClient: false);
        });

        services.TryAddTransient<ICatalogClient>(static serviceProvider =>
            serviceProvider.GetRequiredService<TrendyolClient>().Catalog);
        services.TryAddTransient<IProductsClient>(static serviceProvider =>
            serviceProvider.GetRequiredService<TrendyolClient>().Products);
        services.TryAddTransient<IInventoryClient>(static serviceProvider =>
            serviceProvider.GetRequiredService<TrendyolClient>().Inventory);
        services.TryAddTransient<IOrdersClient>(static serviceProvider =>
            serviceProvider.GetRequiredService<TrendyolClient>().Orders);
        services.TryAddTransient<IReturnsClient>(static serviceProvider =>
            serviceProvider.GetRequiredService<TrendyolClient>().Returns);
        services.TryAddTransient<IQuestionsClient>(static serviceProvider =>
            serviceProvider.GetRequiredService<TrendyolClient>().Questions);
        services.TryAddTransient<IInvoicesClient>(static serviceProvider =>
            serviceProvider.GetRequiredService<TrendyolClient>().Invoices);
        services.TryAddTransient<IWebhooksClient>(static serviceProvider =>
            serviceProvider.GetRequiredService<TrendyolClient>().Webhooks);

        return builder;
    }
}
