using System.Net;
using System.Text;
using Trendyol.Sdk.Inventory;
using Trendyol.Sdk.Invoices;
using Trendyol.Sdk.Orders;
using Trendyol.Sdk.Products;
using Trendyol.Sdk.Questions;
using Trendyol.Sdk.Returns;
using Trendyol.Sdk.UnitTests.TestInfrastructure;
using Trendyol.Sdk.Webhooks;

namespace Trendyol.Sdk.UnitTests.Features;

public sealed class FeatureClientTests
{
    [Fact]
    public async Task CatalogCategoryValuesBuildsDocumentedRouteAndQuery()
    {
        Uri? uri = null;
        using var client = Client((request, _) =>
        {
            uri = request.RequestUri;
            return Json("{\"content\":[]}");
        });

        await client.Catalog.GetCategoryAttributeValuesAsync(12, 34, 2, 100, TestContext.Current.CancellationToken);

        Assert.Equal("https://apigw.trendyol.com/integration/product/categories/12/attributes/34/values?page=2&size=100", uri?.AbsoluteUri);
    }

    [Fact]
    public async Task ProductCreateSerializesCamelCaseBatchRequest()
    {
        string? body = null;
        using var client = Client(async (request, token) =>
        {
            body = await request.Content!.ReadAsStringAsync(token);
            return Json("{\"batchRequestId\":\"batch-1\"}");
        });

        var result = await client.Products.CreateAsync(new CreateProductsRequest
        {
            Items = [new CreateProductItem { Barcode = "123", Title = "T", ProductMainId = "M", BrandId = 1, CategoryId = 2 }],
        }, TestContext.Current.CancellationToken);

        Assert.Equal("batch-1", result.BatchRequestId);
        Assert.Contains("\"productMainId\":\"M\"", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InventoryUsesInventoryRoute()
    {
        Uri? uri = null;
        using var client = Client((request, _) =>
        {
            uri = request.RequestUri;
            return Json("{\"batchRequestId\":\"inventory-1\"}");
        });

        await client.Inventory.UpdatePriceAndInventoryAsync(new PriceAndInventoryRequest
        {
            Items = [new PriceAndInventoryItem { Barcode = "123", Quantity = 5 }],
        }, TestContext.Current.CancellationToken);

        Assert.Equal("/integration/inventory/sellers/1234/products/price-and-inventory", uri?.AbsolutePath);
    }

    [Fact]
    public async Task OrdersStreamEncodesCursorAndStatuses()
    {
        Uri? uri = null;
        using var client = Client((request, _) =>
        {
            uri = request.RequestUri;
            return Json("{\"content\":[],\"hasMore\":false}");
        });

        await client.Orders.StreamShipmentPackagesAsync(new ShipmentPackageStreamFilter
        {
            Cursor = "next/value+1",
            Statuses = ["Created", "Picking"],
            Size = 50,
        }, TestContext.Current.CancellationToken);

        Assert.Equal("?cursor=next%2Fvalue%2B1&size=50&status=Created%2CPicking", uri?.Query);
    }

    [Fact]
    public async Task ClaimIssueUsesMultipartFormData()
    {
        string? mediaType = null;
        string? body = null;
        using var client = Client(async (request, token) =>
        {
            mediaType = request.Content?.Headers.ContentType?.MediaType;
            body = await request.Content!.ReadAsStringAsync(token);
            return Empty();
        });

        await client.Returns.CreateClaimIssueAsync("claim-1", new ClaimIssueRequest
        {
            ClaimIssueReasonId = 7,
            ClaimItemIds = ["item-1"],
            Description = "Damaged",
            Attachments = [new ClaimIssueAttachment { FileName = "proof.txt", Content = Encoding.UTF8.GetBytes("proof"), ContentType = "text/plain" }],
        }, TestContext.Current.CancellationToken);

        Assert.Equal("multipart/form-data", mediaType);
        Assert.Contains("proof.txt", body, StringComparison.Ordinal);
        Assert.Contains("claimItemIdList", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvoiceUploadUsesMultipartAndAcceptsEmptySuccessBody()
    {
        string? body = null;
        using var client = Client(async (request, token) =>
        {
            body = await request.Content!.ReadAsStringAsync(token);
            return Empty();
        });

        await client.Invoices.UploadInvoiceFileAsync(new UploadInvoiceFileRequest
        {
            ShipmentPackageId = 99,
            FileName = "invoice.pdf",
            Content = [1, 2, 3],
        }, TestContext.Current.CancellationToken);

        Assert.Contains("invoice.pdf", body, StringComparison.Ordinal);
        Assert.Contains("99", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task QuestionAnswerUsesDocumentedRoute()
    {
        Uri? uri = null;
        using var client = Client((request, _) =>
        {
            uri = request.RequestUri;
            return Empty();
        });

        await client.Questions.AnswerQuestionAsync(42, new AnswerQuestionRequest { Text = "Yanıt" }, TestContext.Current.CancellationToken);

        Assert.Equal("/integration/qna/sellers/1234/questions/42/answers", uri?.AbsolutePath);
    }

    [Fact]
    public async Task WebhookLifecycleUsesDocumentedRoutes()
    {
        var requests = new List<(HttpMethod Method, string Path)>();
        using var client = Client((request, _) =>
        {
            requests.Add((request.Method, request.RequestUri!.AbsolutePath));
            return request.Method == HttpMethod.Post ? Json("{\"id\":\"hook-8\",\"url\":\"https://example.test/hook\"}") : Empty();
        });

        var webhook = await client.Webhooks.CreateAsync(new WebhookRequest
        {
            Url = "https://example.test/hook",
            AuthenticationType = "API_KEY",
            SubscribedStatuses = ["Created"],
        }, TestContext.Current.CancellationToken);
        await client.Webhooks.ActivateAsync(webhook.Id, TestContext.Current.CancellationToken);
        await client.Webhooks.DeleteAsync(webhook.Id, TestContext.Current.CancellationToken);

        Assert.Equal([
            (HttpMethod.Post, "/integration/sellers/1234/webhooks"),
            (HttpMethod.Put, "/integration/sellers/1234/webhooks/hook-8/activate"),
            (HttpMethod.Delete, "/integration/sellers/1234/webhooks/hook-8")], requests);
    }

    private static TrendyolClient Client(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) =>
        TestClientFactory.Create(new FakeHttpMessageHandler(handler));

    private static TrendyolClient Client(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler) =>
        TestClientFactory.Create(new FakeHttpMessageHandler((request, token) => Task.FromResult(handler(request, token))));

    private static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json"),
    };

    private static HttpResponseMessage Empty() => new(HttpStatusCode.OK) { Content = new ByteArrayContent([]) };
}
