using Microsoft.Extensions.Logging;

namespace Trendyol.Sdk.Internal.Http;

internal static class TrendyolHttpLog
{
    private static readonly Action<ILogger, string, string, Exception?> SendingRequest =
        LoggerMessage.Define<string, string>(
            LogLevel.Debug,
            new EventId(1000, nameof(SendingRequest)),
            "Sending Trendyol operation {Operation} to {RouteTemplate}.");

    private static readonly Action<ILogger, string, int, double, Exception?> RequestCompleted =
        LoggerMessage.Define<string, int, double>(
            LogLevel.Debug,
            new EventId(1001, nameof(RequestCompleted)),
            "Trendyol operation {Operation} completed with HTTP {StatusCode} in {ElapsedMilliseconds} ms.");

    private static readonly Action<ILogger, string, double, Exception?> RequestFailed =
        LoggerMessage.Define<string, double>(
            LogLevel.Warning,
            new EventId(1002, nameof(RequestFailed)),
            "Trendyol operation {Operation} failed before an HTTP response after {ElapsedMilliseconds} ms.");

    internal static void Sending(ILogger logger, string operation, string routeTemplate) =>
        SendingRequest(logger, operation, routeTemplate, null);

    internal static void Completed(ILogger logger, string operation, int statusCode, double elapsedMilliseconds) =>
        RequestCompleted(logger, operation, statusCode, elapsedMilliseconds, null);

    internal static void Failed(ILogger logger, string operation, double elapsedMilliseconds) =>
        RequestFailed(logger, operation, elapsedMilliseconds, null);
}
