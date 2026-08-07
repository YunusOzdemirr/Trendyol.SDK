using System.Diagnostics;

namespace Trendyol.Sdk;

/// <summary>
/// Configures a <see cref="TrendyolClient" /> for the Trendyol Türkiye Marketplace API.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class TrendyolOptions
{
    /// <summary>
    /// Gets or sets the seller identifier issued by Trendyol.
    /// </summary>
    public long SellerId { get; set; }

    /// <summary>
    /// Gets or sets the API key used as the HTTP Basic authentication username.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API secret used as the HTTP Basic authentication password.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string ApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the alphanumeric integrator name included in the required User-Agent header.
    /// </summary>
    /// <remarks>
    /// Use <c>SelfIntegration</c> when the integration software belongs to the seller. Trendyol documents a maximum length of 30 alphanumeric characters.
    /// </remarks>
    public string IntegratorName { get; set; } = "SelfIntegration";

    /// <summary>
    /// Gets or sets the official API environment. The default is production.
    /// </summary>
    public TrendyolEnvironment Environment { get; set; } = TrendyolEnvironment.Production;

    /// <summary>
    /// Gets or sets the HTTP request timeout. The default is 100 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Returns a credential-safe description of this configuration.
    /// </summary>
    /// <returns>A description that never contains the API key or API secret.</returns>
    public override string ToString() =>
        $"TrendyolOptions {{ SellerId = {SellerId}, Environment = {Environment}, IntegratorName = {IntegratorName}, Timeout = {Timeout} }}";

    private string DebuggerDisplay => ToString();
}
