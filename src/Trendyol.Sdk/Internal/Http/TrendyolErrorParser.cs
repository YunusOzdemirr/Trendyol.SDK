using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using System.Text.Json;
using Trendyol.Sdk.Configuration;

namespace Trendyol.Sdk.Internal.Http;

internal static class TrendyolErrorParser
{
    internal static async Task<TrendyolApiException> CreateExceptionAsync(
        HttpResponseMessage response,
        TrendyolOptionsSnapshot options,
        CancellationToken cancellationToken)
    {
        var responseBody = await ReadBodyAsync(response.Content, cancellationToken).ConfigureAwait(false);
        var errors = ParseErrors(responseBody, options);
        var message = CreateMessage(response.StatusCode, response.ReasonPhrase, errors);

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new TrendyolAuthenticationException(message, errors),
            (HttpStatusCode)429 => new TrendyolRateLimitException(
                message,
                errors,
                GetRetryAfter(response.Headers.RetryAfter)),
            _ => new TrendyolApiException(message, response.StatusCode, errors),
        };
    }

    private static async Task<string> ReadBodyAsync(HttpContent content, CancellationToken cancellationToken)
    {
#if NET10_0_OR_GREATER
        using var source = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
        using var source = await content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
        using var destination = new MemoryStream();
        await source.CopyToAsync(destination, 81920, cancellationToken).ConfigureAwait(false);
        return Encoding.UTF8.GetString(destination.ToArray());
    }

    private static IReadOnlyList<TrendyolApiError> ParseErrors(
        string responseBody,
        TrendyolOptionsSnapshot options)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return Array.Empty<TrendyolApiError>();
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return Array.Empty<TrendyolApiError>();
            }

            var parsed = new List<TrendyolApiError>();
            var root = document.RootElement;

            if (TryGetProperty(root, "errors", out var errorsElement) &&
                errorsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var errorElement in errorsElement.EnumerateArray())
                {
                    if (errorElement.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    parsed.Add(new TrendyolApiError(
                        RedactedText(errorElement, "key", options),
                        RedactedText(errorElement, "message", options),
                        RedactedText(errorElement, "errorCode", options)));
                }
            }

            if (parsed.Count == 0)
            {
                var exception = RedactedText(root, "exception", options);
                var message = RedactedText(root, "message", options);
                var errorCode = RedactedText(root, "errorCode", options);

                if (exception is not null || message is not null || errorCode is not null)
                {
                    parsed.Add(new TrendyolApiError(exception, message ?? exception, errorCode));
                }
            }

            return parsed.Count == 0
                ? Array.Empty<TrendyolApiError>()
                : new ReadOnlyCollection<TrendyolApiError>(parsed);
        }
        catch (JsonException)
        {
            return Array.Empty<TrendyolApiError>();
        }
    }

    private static string CreateMessage(
        HttpStatusCode statusCode,
        string? reasonPhrase,
        IReadOnlyList<TrendyolApiError> errors)
    {
        var statusDescription = string.IsNullOrWhiteSpace(reasonPhrase)
            ? statusCode.ToString()
            : reasonPhrase;
        var message = $"Trendyol API returned HTTP {(int)statusCode} ({statusDescription}).";

        var detail = errors.Count > 0 ? errors[0].Message : null;
        return string.IsNullOrWhiteSpace(detail) ? message : $"{message} {detail}";
    }

    private static TimeSpan? GetRetryAfter(System.Net.Http.Headers.RetryConditionHeaderValue? retryAfter)
    {
        if (retryAfter?.Delta is { } delta)
        {
            return delta < TimeSpan.Zero ? TimeSpan.Zero : delta;
        }

        if (retryAfter?.Date is not { } date)
        {
            return null;
        }

        var calculatedDelay = date - DateTimeOffset.UtcNow;
        return calculatedDelay < TimeSpan.Zero ? TimeSpan.Zero : calculatedDelay;
    }

    private static string? RedactedText(
        JsonElement element,
        string propertyName,
        TrendyolOptionsSnapshot options)
    {
        return TryGetProperty(element, propertyName, out var property)
            ? options.Redact(GetText(property))
            : null;
    }

    private static string GetText(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString() ?? string.Empty,
        JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => element.GetRawText(),
        _ => string.Empty,
    };

    private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
