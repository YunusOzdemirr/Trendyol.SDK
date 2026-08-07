using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trendyol.Sdk.UnitTests.TestInfrastructure;

namespace Trendyol.Sdk.UnitTests.DependencyInjection;

public sealed class TrendyolServiceCollectionExtensionsTests
{
    [Fact]
    public async Task AddTrendyolRegistersFactoryManagedClientAndCustomHandler()
    {
        Uri? observedUri = null;
        var handler = new FakeHttpMessageHandler((request, _) =>
        {
            observedUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"value\":\"ok\"}", Encoding.UTF8, "application/json"),
            });
        });
        var services = new ServiceCollection();
        services
            .AddTrendyol(options =>
            {
                options.SellerId = 9876;
                options.ApiKey = "di-key";
                options.ApiSecret = "di-secret";
                options.IntegratorName = "Integrator1";
                options.Environment = TrendyolEnvironment.Stage;
                options.Timeout = TimeSpan.FromSeconds(12);
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        await using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<TrendyolClient>();
        var response = await client.SendAsync<TestResponse>(
            "TestOperation",
            HttpMethod.Get,
            "integration/test",
            "integration/test",
            requestBody: null,
            CancellationToken.None);

        Assert.Equal("ok", response?.Value);
        Assert.Equal(new Uri("https://stageapigw.trendyol.com/integration/test"), observedUri);
    }

    [Fact]
    public void AddTrendyolValidatesOptionsWhenClientIsResolved()
    {
        var services = new ServiceCollection();
        services.AddTrendyol(options =>
        {
            options.SellerId = 1;
            options.ApiKey = string.Empty;
            options.ApiSecret = "secret";
        });
        using var provider = services.BuildServiceProvider();

        Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<TrendyolClient>());
    }

    [Fact]
    public void AddTrendyolRejectsNullArguments()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            TrendyolServiceCollectionExtensions.AddTrendyol(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => services.AddTrendyol(null!));
    }

    private sealed class TestResponse
    {
        public string? Value { get; set; }
    }
}
