namespace Trendyol.Sdk;

/// <summary>
/// Identifies an official Trendyol Türkiye Marketplace API environment.
/// </summary>
public enum TrendyolEnvironment
{
    /// <summary>
    /// The production Trendyol Marketplace API.
    /// </summary>
    Production = 0,

    /// <summary>
    /// The stage Trendyol Marketplace API, which uses separate credentials and requires Trendyol-side access authorization.
    /// </summary>
    Stage = 1,
}
