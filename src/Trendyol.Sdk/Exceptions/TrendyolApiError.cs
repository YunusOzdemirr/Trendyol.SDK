namespace Trendyol.Sdk;

/// <summary>
/// Represents a structured error returned by the Trendyol API.
/// </summary>
public sealed class TrendyolApiError
{
    internal TrendyolApiError(string? key, string? message, string? errorCode)
    {
        Key = key;
        Message = message;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Gets the Trendyol error key, when one was returned.
    /// </summary>
    public string? Key { get; }

    /// <summary>
    /// Gets the credential-redacted Trendyol error message, when one was returned.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Gets the Trendyol error code, when one was returned.
    /// </summary>
    public string? ErrorCode { get; }
}
