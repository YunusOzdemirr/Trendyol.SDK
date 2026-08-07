using System.Net;

namespace Trendyol.Sdk;

/// <summary>
/// The exception thrown when the Trendyol API returns a non-success HTTP response.
/// </summary>
public class TrendyolApiException : Exception
{
    internal TrendyolApiException(
        string message,
        HttpStatusCode statusCode,
        IReadOnlyList<TrendyolApiError> errors)
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }

    /// <summary>
    /// Gets the HTTP status code returned by Trendyol.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the structured, credential-redacted errors parsed from the response.
    /// </summary>
    public IReadOnlyList<TrendyolApiError> Errors { get; }
}
