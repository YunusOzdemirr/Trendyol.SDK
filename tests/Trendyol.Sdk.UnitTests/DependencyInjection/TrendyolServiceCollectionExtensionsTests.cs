using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trendyol.Sdk.Catalog;
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
                Content = new StringContent("[]", Encoding.UTF8, "application/json"),
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
        var response = await client.Catalog.SearchBrandsByNameAsync(
            "Milla",
            TestContext.Current.CancellationToken);

        Assert.Empty(response);
        Assert.Equal(
            new Uri("https://stageapigw.trendyol.com/integration/product/brands/by-name?name=Milla"),
            observedUri);
    }

    [Fact]
    public async Task AddTrendyolRegistersCatalogClientInterface()
    {
        var handler = new FakeHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "[{\"id\":40,\"name\":\"TRENDYOLMİLLA\",\"luxe\":false}]",
                    Encoding.UTF8,
                    "application/json"),
            }));
        var services = new ServiceCollection();
        services
            .AddTrendyol(options =>
            {
                options.SellerId = 9876;
                options.ApiKey = "di-key";
                options.ApiSecret = "di-secret";
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler);
        await using var provider = services.BuildServiceProvider();

        var catalog = provider.GetRequiredService<ICatalogClient>();
        var brands = await catalog.SearchBrandsByNameAsync(
            "TRENDYOLMİLLA",
            TestContext.Current.CancellationToken);

        var brand = Assert.Single(brands);
        Assert.Equal(40, brand.Id);
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
}
