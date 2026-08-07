using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Trendyol.Sdk.UnitTests.TestInfrastructure;

namespace Trendyol.Sdk.UnitTests.Internal;

public sealed class TrendyolHttpTransportTests
{
    [Fact]
    public async Task RequestContainsExpectedHostAuthenticationAndUserAgent()
    {
        Uri? requestUri = null;
        AuthenticationHeaderValue? authorization = null;
        string? userAgent = null;
        var handler = SuccessHandler(request =>
        {
            requestUri = request.RequestUri;
            authorization = request.Headers.Authorization;
            userAgent = string.Join(" ", request.Headers.GetValues("User-Agent"));
        });
        using var client = TestClientFactory.Create(handler);

        var result = await client.SendAsync<TestResponse>(
            "TestOperation",
            HttpMethod.Get,
            "integration/test?value=1",
            "integration/test",
            requestBody: null,
            CancellationToken.None);

        Assert.Equal(new Uri("https://apigw.trendyol.com/integration/test?value=1"), requestUri);
        Assert.Equal("Basic", authorization?.Scheme);
        Assert.Equal(
            Convert.ToBase64String(Encoding.UTF8.GetBytes("test-api-key:test-api-secret")),
            authorization?.Parameter);
        Assert.Equal("1234 - SelfIntegration", userAgent);
        Assert.Equal("ok", result?.Value);
    }

    [Fact]
    public async Task StageEnvironmentUsesStageHost()
    {
        Uri? requestUri = null;
        var options = TestClientFactory.ValidOptions();
        options.Environment = TrendyolEnvironment.Stage;
        var handler = SuccessHandler(request => requestUri = request.RequestUri);
        using var client = TestClientFactory.Create(handler, options);

        await client.SendAsync<TestResponse>(
            "TestOperation",
            HttpMethod.Get,
            "integration/test",
            "integration/test",
            requestBody: null,
            CancellationToken.None);

        Assert.Equal(new Uri("https://stageapigw.trendyol.com/integration/test"), requestUri);
    }

    [Fact]
    public async Task OneClientReusesItsHttpPipeline()
    {
        var handler = SuccessHandler();
        using var client = TestClientFactory.Create(handler);

        await SendTestRequestAsync(client, TestContext.Current.CancellationToken);
        await SendTestRequestAsync(client, TestContext.Current.CancellationToken);

        Assert.Equal(2, handler.SendCount);
    }

    [Fact]
    public async Task RequestBodyUsesCentralJsonSettings()
    {
        string? body = null;
        string? contentType = null;
        var handler = new FakeHttpMessageHandler(async (request, cancellationToken) =>
        {
            body = await request.Content!.ReadAsStringAsync(cancellationToken);
            contentType = request.Content.Headers.ContentType?.ToString();
            return JsonResponse(HttpStatusCode.OK, "{\"VALUE\":\"accepted\",\"unknown\":true}");
        });
        using var client = TestClientFactory.Create(handler);

        var result = await client.SendAsync<TestResponse>(
            "TestOperation",
            HttpMethod.Post,
            "integration/test",
            "integration/test",
            new TestRequest { RequiredValue = "sent", OptionalValue = null },
            CancellationToken.None);

        Assert.Equal("{\"requiredValue\":\"sent\"}", body);
        Assert.Equal("application/json; charset=utf-8", contentType);
        Assert.Equal("accepted", result?.Value);
    }

    [Fact]
    public async Task CallerCancellationIsPreserved()
    {
        var handler = new FakeHttpMessageHandler(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return JsonResponse(HttpStatusCode.OK, "{}");
        });
        using var client = TestClientFactory.Create(handler);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            SendTestRequestAsync(client, cancellation.Token));

        Assert.Equal(cancellation.Token, exception.CancellationToken);
    }

    [Fact]
    public async Task AbsoluteRequestUriIsRejectedBeforeSendingCredentials()
    {
        var handler = SuccessHandler();
        using var client = TestClientFactory.Create(handler);

        await Assert.ThrowsAsync<ArgumentException>(() => client.SendAsync<TestResponse>(
            "TestOperation",
            HttpMethod.Get,
            "https://example.com/collect",
            "integration/test",
            requestBody: null,
            CancellationToken.None));

        Assert.Equal(0, handler.SendCount);
    }

    [Fact]
    public async Task UnauthorizedResponseMapsStructuredErrorAndRedactsCredentials()
    {
        const string response = """
            {
              "errors": [
                {
                  "key": "invalid.credentials",
                  "message": "test-api-key and test-api-secret are invalid",
                  "errorCode": "401"
                }
              ]
            }
            """;
        var handler = ResponseHandler(HttpStatusCode.Unauthorized, response);
        using var client = TestClientFactory.Create(handler);

        var exception = await Assert.ThrowsAsync<TrendyolAuthenticationException>(() =>
            SendTestRequestAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        var error = Assert.Single(exception.Errors);
        Assert.Equal("invalid.credentials", error.Key);
        Assert.Equal("401", error.ErrorCode);
        Assert.DoesNotContain("test-api-key", exception.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("test-api-secret", exception.ToString(), StringComparison.Ordinal);
        Assert.Equal("[REDACTED] and [REDACTED] are invalid", error.Message);
    }

    [Fact]
    public async Task AuthenticationExceptionFormIsParsedCaseInsensitively()
    {
        const string response = "{\"Exception\":\"ClientApiAuthenticationException\",\"Message\":\"Unauthorized\"}";
        var handler = ResponseHandler(HttpStatusCode.Unauthorized, response);
        using var client = TestClientFactory.Create(handler);

        var exception = await Assert.ThrowsAsync<TrendyolAuthenticationException>(() =>
            SendTestRequestAsync(client, TestContext.Current.CancellationToken));

        var error = Assert.Single(exception.Errors);
        Assert.Equal("ClientApiAuthenticationException", error.Key);
        Assert.Equal("Unauthorized", error.Message);
    }

    [Fact]
    public async Task RateLimitResponseMapsRetryAfterWithoutRetrying()
    {
        var handler = new FakeHttpMessageHandler((_, _) =>
        {
            var response = JsonResponse((HttpStatusCode)429, "{\"message\":\"too.many.requests\"}");
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(7));
            return Task.FromResult(response);
        });
        using var client = TestClientFactory.Create(handler);

        var exception = await Assert.ThrowsAsync<TrendyolRateLimitException>(() =>
            SendTestRequestAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(TimeSpan.FromSeconds(7), exception.RetryAfter);
        Assert.Equal(1, handler.SendCount);
    }

    [Fact]
    public async Task UnknownErrorShapeMapsGenericApiExceptionWithoutRawBody()
    {
        const string rawBody = "customer-specific raw response";
        var handler = ResponseHandler(HttpStatusCode.BadRequest, rawBody, "text/plain");
        using var client = TestClientFactory.Create(handler);

        var exception = await Assert.ThrowsAsync<TrendyolApiException>(() =>
            SendTestRequestAsync(client, TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Empty(exception.Errors);
        Assert.DoesNotContain(rawBody, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LogsContainOnlySafeOperationMetadata()
    {
        var logger = new ListLogger();
        var handler = ResponseHandler(
            HttpStatusCode.BadRequest,
            "{\"message\":\"test-api-key test-api-secret customer-value\"}");
        using var client = TestClientFactory.Create(handler, logger: logger);

        await Assert.ThrowsAsync<TrendyolApiException>(() => client.SendAsync<TestResponse>(
            "SafeOperation",
            HttpMethod.Get,
            "integration/test?customer=customer-value",
            "integration/test",
            requestBody: null,
            CancellationToken.None));

        var combinedLogs = string.Join(Environment.NewLine, logger.Messages);
        Assert.Contains("SafeOperation", combinedLogs, StringComparison.Ordinal);
        Assert.DoesNotContain("test-api-key", combinedLogs, StringComparison.Ordinal);
        Assert.DoesNotContain("test-api-secret", combinedLogs, StringComparison.Ordinal);
        Assert.DoesNotContain("customer-value", combinedLogs, StringComparison.Ordinal);
    }

    [Fact]
    public void DisposingOwnedClientDisposesHandler()
    {
        var handler = SuccessHandler();
        var client = TestClientFactory.Create(handler, disposeHttpClient: true);

        client.Dispose();
        client.Dispose();

        Assert.True(handler.IsDisposed);
    }

    private static Task<TestResponse?> SendTestRequestAsync(
        TrendyolClient client,
        CancellationToken cancellationToken = default) => client.SendAsync<TestResponse>(
            "TestOperation",
            HttpMethod.Get,
            "integration/test",
            "integration/test",
            requestBody: null,
            cancellationToken);

    private static FakeHttpMessageHandler SuccessHandler(Action<HttpRequestMessage>? inspect = null) =>
        new((request, _) =>
        {
            inspect?.Invoke(request);
            return Task.FromResult(JsonResponse(HttpStatusCode.OK, "{\"value\":\"ok\"}"));
        });

    private static FakeHttpMessageHandler ResponseHandler(
        HttpStatusCode statusCode,
        string body,
        string mediaType = "application/json") => new((_, _) =>
            Task.FromResult(JsonResponse(statusCode, body, mediaType)));

    private static HttpResponseMessage JsonResponse(
        HttpStatusCode statusCode,
        string body,
        string mediaType = "application/json") => new(statusCode)
        {
            Content = new StringContent(body, Encoding.UTF8, mediaType),
        };

    private sealed class TestRequest
    {
        public string RequiredValue { get; set; } = string.Empty;

        public string? OptionalValue { get; set; }
    }

    private sealed class TestResponse
    {
        public string? Value { get; set; }
    }
}
