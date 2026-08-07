using System.Net;

namespace Trendyol.Sdk;

/// <summary>
/// The exception thrown when Trendyol rejects API authentication.
/// </summary>
public sealed class TrendyolAuthenticationException : TrendyolApiException
{
    internal TrendyolAuthenticationException(string message, IReadOnlyList<TrendyolApiError> errors)
        : base(message, HttpStatusCode.Unauthorized, errors)
    {
    }
}
