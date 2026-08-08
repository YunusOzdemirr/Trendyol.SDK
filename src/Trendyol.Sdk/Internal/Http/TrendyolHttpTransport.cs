using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Trendyol.Sdk.Configuration;
using Trendyol.Sdk.Internal.Serialization;

namespace Trendyol.Sdk.Internal.Http;

internal sealed class TrendyolHttpTransport
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly TrendyolOptionsSnapshot _options;

    internal TrendyolHttpTransport(
        HttpClient httpClient,
        TrendyolOptionsSnapshot options,
        ILogger logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    internal async Task<TResponse?> SendAsync<TResponse>(
        string operation,
        HttpMethod method,
        string relativeUri,
        string routeTemplate,
        object? requestBody,
        CancellationToken cancellationToken)
    {
        HttpContent? content = null;
        if (requestBody is not null)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(requestBody, requestBody.GetType(), TrendyolJson.Options);
            content = new ByteArrayContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
        }

        return await SendCoreAsync<TResponse>(
            operation,
            method,
            relativeUri,
            routeTemplate,
            content,
            deserializeResponse: true,
            cancellationToken).ConfigureAwait(false);
    }

    internal async Task SendAsync(
        string operation,
        HttpMethod method,
        string relativeUri,
        string routeTemplate,
        object? requestBody,
        CancellationToken cancellationToken)
    {
        HttpContent? content = null;
        if (requestBody is not null)
        {
            var json = JsonSerializer.SerializeToUtf8Bytes(requestBody, requestBody.GetType(), TrendyolJson.Options);
            content = new ByteArrayContent(json);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
        }

        await SendCoreAsync<object>(
            operation,
            method,
            relativeUri,
            routeTemplate,
            content,
            deserializeResponse: false,
            cancellationToken).ConfigureAwait(false);
    }

    internal Task<TResponse?> SendContentAsync<TResponse>(
        string operation,
        HttpMethod method,
        string relativeUri,
        string routeTemplate,
        HttpContent content,
        CancellationToken cancellationToken) =>
        SendCoreAsync<TResponse>(operation, method, relativeUri, routeTemplate, content, true, cancellationToken);

    internal async Task SendContentAsync(
        string operation,
        HttpMethod method,
        string relativeUri,
        string routeTemplate,
        HttpContent content,
        CancellationToken cancellationToken)
    {
        await SendCoreAsync<object>(
            operation,
            method,
            relativeUri,
            routeTemplate,
            content,
            deserializeResponse: false,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<TResponse?> SendCoreAsync<TResponse>(
        string operation,
        HttpMethod method,
        string relativeUri,
        string routeTemplate,
        HttpContent? content,
        bool deserializeResponse,
        CancellationToken cancellationToken)
    {
        ValidateRelativeUri(relativeUri);

        using var request = new HttpRequestMessage(method, relativeUri) { Content = content };
        AddRequiredHeaders(request);

        TrendyolHttpLog.Sending(_logger, operation, routeTemplate);
        var stopwatch = Stopwatch.StartNew();

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            TrendyolHttpLog.Failed(_logger, operation, stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }

        using (response)
        {
            TrendyolHttpLog.Completed(
                _logger,
                operation,
                (int)response.StatusCode,
                stopwatch.Elapsed.TotalMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                throw await TrendyolErrorParser.CreateExceptionAsync(
                    response,
                    _options,
                    cancellationToken).ConfigureAwait(false);
            }

            if (!deserializeResponse || response.Content.Headers.ContentLength == 0)
            {
                return default;
            }

#if NET10_0_OR_GREATER
            var responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
            var responseBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
            if (responseBytes.Length == 0)
            {
                return default;
            }

            return JsonSerializer.Deserialize<TResponse>(responseBytes, TrendyolJson.Options);
        }
    }

    private static void ValidateRelativeUri(string relativeUri)
    {
        if (string.IsNullOrWhiteSpace(relativeUri))
        {
            throw new ArgumentException("A relative Trendyol API URI is required.", nameof(relativeUri));
        }

        if (Uri.TryCreate(relativeUri, UriKind.Absolute, out _))
        {
            throw new ArgumentException(
                "Absolute request URIs are not accepted because credentials may only be sent to the configured Trendyol host.",
                nameof(relativeUri));
        }
    }

    private void AddRequiredHeaders(HttpRequestMessage request)
    {
        var credentials = Encoding.UTF8.GetBytes($"{_options.ApiKey}:{_options.ApiSecret}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
        request.Headers.TryAddWithoutValidation("User-Agent", _options.UserAgent);
    }
}
