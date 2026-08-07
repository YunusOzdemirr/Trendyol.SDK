using System.Net;

namespace Trendyol.Sdk;

/// <summary>
/// The exception thrown when Trendyol rejects a request because a rate limit was exceeded.
/// </summary>
public sealed class TrendyolRateLimitException : TrendyolApiException
{
    internal TrendyolRateLimitException(
        string message,
        IReadOnlyList<TrendyolApiError> errors,
        TimeSpan? retryAfter)
        : base(message, (HttpStatusCode)429, errors)
    {
        RetryAfter = retryAfter;
    }

    /// <summary>
    /// Gets the delay requested by the standard Retry-After response header, when present and valid.
    /// </summary>
    public TimeSpan? RetryAfter { get; }
}
