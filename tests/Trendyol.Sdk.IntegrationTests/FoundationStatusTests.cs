namespace Trendyol.Sdk.IntegrationTests;

public sealed class FoundationStatusTests
{
    [Fact]
    public async Task CategoryTreeCanBeReadFromStageWhenExplicitlyConfigured()
    {
        var sellerIdText = Environment.GetEnvironmentVariable("TRENDYOL_STAGE_SELLER_ID");
        var apiKey = Environment.GetEnvironmentVariable("TRENDYOL_STAGE_API_KEY");
        var apiSecret = Environment.GetEnvironmentVariable("TRENDYOL_STAGE_API_SECRET");
        if (!long.TryParse(sellerIdText, out var sellerId) || sellerId <= 0 ||
            string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
        {
            Assert.Skip("Set TRENDYOL_STAGE_SELLER_ID, TRENDYOL_STAGE_API_KEY, and TRENDYOL_STAGE_API_SECRET to run live Stage tests.");
        }

        using var client = new TrendyolClient(new TrendyolOptions
        {
            SellerId = sellerId,
            ApiKey = apiKey,
            ApiSecret = apiSecret,
            Environment = TrendyolEnvironment.Stage,
        });

        var categories = await client.Catalog.GetCategoryTreeAsync(TestContext.Current.CancellationToken);
        Assert.NotEmpty(categories);
    }
}
