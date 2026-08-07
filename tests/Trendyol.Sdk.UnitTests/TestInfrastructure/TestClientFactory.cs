using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Trendyol.Sdk.Configuration;

namespace Trendyol.Sdk.UnitTests.TestInfrastructure;

internal static class TestClientFactory
{
    internal static TrendyolClient Create(
        FakeHttpMessageHandler handler,
        TrendyolOptions? options = null,
        ILogger? logger = null,
        bool disposeHttpClient = true)
    {
        var snapshot = TrendyolOptionsSnapshot.Create(options ?? ValidOptions());
        var httpClient = new HttpClient(handler, disposeHandler: true)
        {
            BaseAddress = snapshot.BaseAddress,
            Timeout = snapshot.Timeout,
        };

        return new TrendyolClient(
            httpClient,
            snapshot,
            logger ?? NullLogger.Instance,
            disposeHttpClient);
    }

    internal static TrendyolOptions ValidOptions() => new()
    {
        SellerId = 1234,
        ApiKey = "test-api-key",
        ApiSecret = "test-api-secret",
        IntegratorName = "SelfIntegration",
        Environment = TrendyolEnvironment.Production,
        Timeout = TimeSpan.FromSeconds(15),
    };
}
