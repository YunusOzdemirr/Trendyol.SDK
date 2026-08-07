using System.Diagnostics;
using System.Reflection;
using Trendyol.Sdk.Configuration;
using Trendyol.Sdk.UnitTests.TestInfrastructure;

namespace Trendyol.Sdk.UnitTests.Configuration;

public sealed class TrendyolOptionsTests
{
    [Fact]
    public void ValidOptionsCreateImmutableSnapshot()
    {
        var options = TestClientFactory.ValidOptions();

        var snapshot = TrendyolOptionsSnapshot.Create(options);
        options.ApiKey = "changed-key";
        options.ApiSecret = "changed-secret";
        options.Environment = TrendyolEnvironment.Stage;

        Assert.Equal("test-api-key", snapshot.ApiKey);
        Assert.Equal("test-api-secret", snapshot.ApiSecret);
        Assert.Equal(new Uri("https://apigw.trendyol.com/"), snapshot.BaseAddress);
    }

    [Fact]
    public void ToStringDoesNotExposeCredentials()
    {
        var options = TestClientFactory.ValidOptions();

        var description = options.ToString();

        Assert.DoesNotContain(options.ApiKey, description, StringComparison.Ordinal);
        Assert.DoesNotContain(options.ApiSecret, description, StringComparison.Ordinal);
        Assert.Contains("SellerId = 1234", description, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Name With Spaces")]
    [InlineData("Name-With-Dash")]
    [InlineData("1234567890123456789012345678901")]
    [InlineData("Şirket")]
    public void InvalidIntegratorNameIsRejected(string integratorName)
    {
        var options = TestClientFactory.ValidOptions();
        options.IntegratorName = integratorName;

        Assert.Throws<ArgumentException>(() => TrendyolOptionsSnapshot.Create(options));
    }

    [Fact]
    public void InvalidRequiredValuesAreRejected()
    {
        var invalidSeller = TestClientFactory.ValidOptions();
        invalidSeller.SellerId = 0;
        var invalidKey = TestClientFactory.ValidOptions();
        invalidKey.ApiKey = " ";
        var invalidSecret = TestClientFactory.ValidOptions();
        invalidSecret.ApiSecret = string.Empty;
        var invalidTimeout = TestClientFactory.ValidOptions();
        invalidTimeout.Timeout = TimeSpan.Zero;

        Assert.Throws<ArgumentOutOfRangeException>(() => TrendyolOptionsSnapshot.Create(invalidSeller));
        Assert.Throws<ArgumentException>(() => TrendyolOptionsSnapshot.Create(invalidKey));
        Assert.Throws<ArgumentException>(() => TrendyolOptionsSnapshot.Create(invalidSecret));
        Assert.Throws<ArgumentOutOfRangeException>(() => TrendyolOptionsSnapshot.Create(invalidTimeout));
    }

    [Fact]
    public void CredentialPropertiesAreHiddenFromDebuggerDisplay()
    {
        var apiKeyAttribute = typeof(TrendyolOptions)
            .GetProperty(nameof(TrendyolOptions.ApiKey))!
            .GetCustomAttribute<DebuggerBrowsableAttribute>();
        var apiSecretAttribute = typeof(TrendyolOptions)
            .GetProperty(nameof(TrendyolOptions.ApiSecret))!
            .GetCustomAttribute<DebuggerBrowsableAttribute>();

        Assert.Equal(DebuggerBrowsableState.Never, apiKeyAttribute?.State);
        Assert.Equal(DebuggerBrowsableState.Never, apiSecretAttribute?.State);
    }

    [Fact]
    public void RedactionRemovesEveryCredentialOccurrence()
    {
        var snapshot = TrendyolOptionsSnapshot.Create(TestClientFactory.ValidOptions());

        var redacted = snapshot.Redact(
            "test-api-key and test-api-secret then test-api-key again");

        Assert.Equal("[REDACTED] and [REDACTED] then [REDACTED] again", redacted);
    }
}
