using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Trendyol.Sdk.UnitTests.TestInfrastructure;

namespace Trendyol.Sdk.UnitTests.Catalog;

public sealed class CatalogClientTests
{
    [Fact]
    public async Task SearchBrandsByNameSendsExpectedGetRequestAndEncodedName()
    {
        Uri? observedUri = null;
        HttpMethod? observedMethod = null;
        HttpContent? observedContent = null;
        AuthenticationHeaderValue? observedAuthorization = null;
        string? observedUserAgent = null;
        var handler = new FakeHttpMessageHandler((request, _) =>
        {
            observedUri = request.RequestUri;
            observedMethod = request.Method;
            observedContent = request.Content;
            observedAuthorization = request.Headers.Authorization;
            observedUserAgent = string.Join(" ", request.Headers.GetValues("User-Agent"));
            return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[]"));
        });
        using var client = TestClientFactory.Create(handler);

        await client.Catalog.SearchBrandsByNameAsync(
            "TRENDYOLMİLLA & A+B/C",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpMethod.Get, observedMethod);
        Assert.Null(observedContent);
        Assert.Equal("Basic", observedAuthorization?.Scheme);
        Assert.Equal(
            Convert.ToBase64String(Encoding.UTF8.GetBytes("test-api-key:test-api-secret")),
            observedAuthorization?.Parameter);
        Assert.Equal("1234 - SelfIntegration", observedUserAgent);
        Assert.Equal(
            "https://apigw.trendyol.com/integration/product/brands/by-name?name=TRENDYOLM%C4%B0LLA%20%26%20A%2BB%2FC",
            observedUri?.AbsoluteUri);
    }

    [Fact]
    public async Task SearchBrandsByNameMapsDocumentedResponseAndToleratesEvolution()
    {
        const string response = """
            [
              { "id": 40, "name": "TRENDYOLMİLLA", "luxe": false, "futureField": "ignored" },
              { "ID": 41, "NAME": "Milla" }
            ]
            """;
        var handler = ResponseHandler(HttpStatusCode.OK, response);
        using var client = TestClientFactory.Create(handler);

        var brands = await client.Catalog.SearchBrandsByNameAsync(
            "TRENDYOLMİLLA",
            TestContext.Current.CancellationToken);

        Assert.Collection(
            brands,
            brand =>
            {
                Assert.Equal(40, brand.Id);
                Assert.Equal("TRENDYOLMİLLA", brand.Name);
                Assert.False(brand.Luxe);
            },
            brand =>
            {
                Assert.Equal(41, brand.Id);
                Assert.Equal("Milla", brand.Name);
                Assert.Null(brand.Luxe);
            });
    }

    [Fact]
    public async Task SearchBrandsByNameReturnsEmptyListForEmptyArray()
    {
        var handler = ResponseHandler(HttpStatusCode.OK, "[]");
        using var client = TestClientFactory.Create(handler);

        var brands = await client.Catalog.SearchBrandsByNameAsync(
            "UnknownBrand",
            TestContext.Current.CancellationToken);

        Assert.Empty(brands);
    }

    [Fact]
    public async Task SearchBrandsByNameRejectsNullBeforeSendingRequest()
    {
        var handler = ResponseHandler(HttpStatusCode.OK, "[]");
        using var client = TestClientFactory.Create(handler);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.Catalog.SearchBrandsByNameAsync(null!, TestContext.Current.CancellationToken));

        Assert.Equal(0, handler.SendCount);
    }

    [Fact]
    public async Task SearchBrandsByNameDoesNotNormalizeEmptyOrWhitespaceValues()
    {
        var observedQueries = new List<string>();
        var handler = new FakeHttpMessageHandler((request, _) =>
        {
            observedQueries.Add(request.RequestUri!.Query);
            return Task.FromResult(JsonResponse(HttpStatusCode.OK, "[]"));
        });
        using var client = TestClientFactory.Create(handler);

        await client.Catalog.SearchBrandsByNameAsync(string.Empty, TestContext.Current.CancellationToken);
        await client.Catalog.SearchBrandsByNameAsync(" ", TestContext.Current.CancellationToken);

        Assert.Equal(["?name=", "?name=%20"], observedQueries);
    }

    [Fact]
    public async Task SearchBrandsByNameRejectsEmptySuccessBody()
    {
        var handler = ResponseHandler(HttpStatusCode.OK, string.Empty);
        using var client = TestClientFactory.Create(handler);

        var exception = await Assert.ThrowsAsync<JsonException>(() =>
            client.Catalog.SearchBrandsByNameAsync("Milla", TestContext.Current.CancellationToken));

        Assert.Equal("Trendyol returned an empty brand-search response.", exception.Message);
    }

    [Fact]
    public async Task SearchBrandsByNameRejectsBrandWithoutName()
    {
        var handler = ResponseHandler(HttpStatusCode.OK, "[{\"id\":40}]");
        using var client = TestClientFactory.Create(handler);

        var exception = await Assert.ThrowsAsync<JsonException>(() =>
            client.Catalog.SearchBrandsByNameAsync("Milla", TestContext.Current.CancellationToken));

        Assert.Equal("Trendyol returned a brand without a name.", exception.Message);
    }

    [Fact]
    public async Task SearchBrandsByNamePreservesCallerCancellation()
    {
        CancellationToken observedToken = default;
        var handler = new FakeHttpMessageHandler(async (_, cancellationToken) =>
        {
            observedToken = cancellationToken;
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return JsonResponse(HttpStatusCode.OK, "[]");
        });
        using var client = TestClientFactory.Create(handler);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.Catalog.SearchBrandsByNameAsync("Milla", cancellation.Token));

        Assert.True(observedToken.IsCancellationRequested);
        Assert.True(exception.CancellationToken.IsCancellationRequested);
    }

    [Fact]
    public async Task SearchBrandsByNameUsesExistingAuthenticationErrorMapping()
    {
        var handler = ResponseHandler(
            HttpStatusCode.Unauthorized,
            "{\"exception\":\"ClientApiAuthenticationException\",\"message\":\"Unauthorized\"}");
        using var client = TestClientFactory.Create(handler);

        var exception = await Assert.ThrowsAsync<TrendyolAuthenticationException>(() =>
            client.Catalog.SearchBrandsByNameAsync("Milla", TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }

    [Fact]
    public async Task SearchBrandsByNameUsesExistingRateLimitMappingWithoutRetrying()
    {
        var handler = new FakeHttpMessageHandler((_, _) =>
        {
            var response = JsonResponse((HttpStatusCode)429, "{\"message\":\"Too many requests\"}");
            response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(9));
            return Task.FromResult(response);
        });
        using var client = TestClientFactory.Create(handler);

        var exception = await Assert.ThrowsAsync<TrendyolRateLimitException>(() =>
            client.Catalog.SearchBrandsByNameAsync("Milla", TestContext.Current.CancellationToken));

        Assert.Equal(TimeSpan.FromSeconds(9), exception.RetryAfter);
        Assert.Equal(1, handler.SendCount);
    }

    [Fact]
    public async Task SearchBrandsByNameLogsNoQueryOrBrandValue()
    {
        var logger = new ListLogger();
        var handler = ResponseHandler(HttpStatusCode.OK, "[]");
        using var client = TestClientFactory.Create(handler, logger: logger);

        await client.Catalog.SearchBrandsByNameAsync(
            "Private Brand & Value",
            TestContext.Current.CancellationToken);

        var combinedLogs = string.Join(Environment.NewLine, logger.Messages);
        Assert.Contains("Catalog.SearchBrandsByName", combinedLogs, StringComparison.Ordinal);
        Assert.Contains("integration/product/brands/by-name", combinedLogs, StringComparison.Ordinal);
        Assert.DoesNotContain("Private Brand", combinedLogs, StringComparison.Ordinal);
        Assert.DoesNotContain("%26", combinedLogs, StringComparison.Ordinal);
        Assert.DoesNotContain("?name=", combinedLogs, StringComparison.Ordinal);
    }

    private static FakeHttpMessageHandler ResponseHandler(HttpStatusCode statusCode, string body) =>
        new((_, _) => Task.FromResult(JsonResponse(statusCode, body)));

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string body) => new(statusCode)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json"),
    };
}
