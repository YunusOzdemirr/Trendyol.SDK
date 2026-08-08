using System.Text.Json;
using Trendyol.Sdk.Internal;
using Trendyol.Sdk.Internal.Http;

namespace Trendyol.Sdk.Products;

internal sealed class ProductsClient : IProductsClient
{
    private readonly TrendyolClient _client;

    internal ProductsClient(TrendyolClient client) => _client = client;

    public Task<BatchRequestReference> CreateAsync(CreateProductsRequest request, CancellationToken cancellationToken = default) =>
        SendBatchAsync("Products.Create", HttpMethod.Post, "v2/products", request, request?.Items, cancellationToken);

    public Task<BatchRequestReference> UpdateUnapprovedAsync(UpdateUnapprovedProductsRequest request, CancellationToken cancellationToken = default) =>
        SendBatchAsync("Products.UpdateUnapproved", HttpMethod.Post, "products/unapproved-bulk-update", request, request?.Items, cancellationToken);

    public Task<BatchRequestReference> UpdateContentAsync(UpdateProductContentRequest request, CancellationToken cancellationToken = default) =>
        SendBatchAsync("Products.UpdateContent", HttpMethod.Post, "products/content-bulk-update", request, request?.Items, cancellationToken);

    public Task<BatchRequestReference> UpdateVariantsAsync(UpdateProductVariantsRequest request, CancellationToken cancellationToken = default) =>
        SendBatchAsync("Products.UpdateVariants", HttpMethod.Post, "products/variant-bulk-update", request, request?.Items, cancellationToken);

    public Task<BatchRequestReference> UpdateDeliveryAsync(UpdateProductDeliveryRequest request, CancellationToken cancellationToken = default) =>
        SendBatchAsync("Products.UpdateDelivery", HttpMethod.Post, "products/delivery-info-bulk-update", request, request?.Items, cancellationToken);

    public async Task<ProductBase> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotEmpty(barcode, nameof(barcode));
        var uri = $"integration/product/sellers/{_client.SellerId}/product/{Uri.EscapeDataString(barcode)}";
        return await RequiredAsync<ProductBase>(
            "Products.GetByBarcode", HttpMethod.Get, uri,
            "integration/product/sellers/{sellerId}/product/{barcode}", null, cancellationToken).ConfigureAwait(false);
    }

    public Task<ProductPage> GetUnapprovedAsync(ProductFilter? filter = null, CancellationToken cancellationToken = default) =>
        GetPageAsync("Products.GetUnapproved", "products/unapproved", filter, 1000, cancellationToken);

    public Task<ProductPage> GetApprovedAsync(ProductFilter? filter = null, CancellationToken cancellationToken = default) =>
        GetPageAsync("Products.GetApproved", "products/approved", filter, 100, cancellationToken);

    public Task<ProductPage> GetApprovedInventoryAndPriceAsync(ProductFilter? filter = null, CancellationToken cancellationToken = default) =>
        GetPageAsync("Products.GetApprovedInventoryAndPrice", "products/approved/inventory-and-price", filter, 1000, cancellationToken);

    public Task<BatchRequestReference> DeleteAsync(ProductBarcodeRequest request, CancellationToken cancellationToken = default) =>
        SendBatchAsync("Products.Delete", HttpMethod.Delete, "products", request, request?.Items, cancellationToken);

    public Task<BatchRequestReference> SetArchiveStateAsync(ArchiveProductsRequest request, CancellationToken cancellationToken = default) =>
        SendBatchAsync("Products.SetArchiveState", HttpMethod.Put, "products/archive-state", request, request?.Items, cancellationToken);

    public Task<BatchRequestReference> UnlockAsync(ProductBarcodeRequest request, CancellationToken cancellationToken = default) =>
        SendBatchAsync("Products.Unlock", HttpMethod.Put, "products/unlock", request, request?.Items, cancellationToken);

    public async Task<BuyboxResponse> GetBuyboxInformationAsync(BuyboxRequest request, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotNull(request, nameof(request));
        TrendyolGuard.Count(request.Barcodes, nameof(request.Barcodes), 10);
        return await RequiredAsync<BuyboxResponse>(
            "Products.GetBuyboxInformation", HttpMethod.Post, SellerRoute("products/buybox-information"),
            "integration/product/sellers/{sellerId}/products/buybox-information", request, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<BatchRequestResult> GetBatchResultAsync(string batchRequestId, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotEmpty(batchRequestId, nameof(batchRequestId));
        var uri = SellerRoute($"products/batch-requests/{Uri.EscapeDataString(batchRequestId)}");
        return await RequiredAsync<BatchRequestResult>(
            "Products.GetBatchResult", HttpMethod.Get, uri,
            "integration/product/sellers/{sellerId}/products/batch-requests/{batchRequestId}", null, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ProductUpdateAudit>> GetUpdateAuditsAsync(long contentId, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.Positive(contentId, nameof(contentId));
        var uri = SellerRoute($"products/{contentId}/update-audits");
        var result = await _client.SendAsync<List<ProductUpdateAudit>>(
            "Products.GetUpdateAudits", HttpMethod.Get, uri,
            "integration/product/sellers/{sellerId}/products/{contentId}/update-audits", null, cancellationToken)
            .ConfigureAwait(false);
        return result ?? throw new JsonException("Trendyol returned an empty product-update-audits response.");
    }

    public async Task<CreateProductVideoResponse> CreateVideoAsync(
        CreateProductVideoRequest request,
        CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotNull(request, nameof(request));
        TrendyolGuard.NotEmpty(request.Title, nameof(request.Title));
        TrendyolGuard.NotEmpty(request.VideoUrl, nameof(request.VideoUrl));
        TrendyolGuard.Count(request.ProductContentIds, nameof(request.ProductContentIds), 100);
        return await RequiredAsync<CreateProductVideoResponse>(
            "Products.CreateVideo", HttpMethod.Post,
            $"integration/video/sellers/{_client.SellerId}/videos",
            "integration/video/sellers/{sellerId}/videos", request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ProductVideoPage> GetVideosAsync(
        ProductVideoFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        filter ??= new ProductVideoFilter();
        if (filter.Page < 0 || filter.Size is <= 0 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(filter));
        }

        var uri = new TrendyolQuery($"integration/video/sellers/{_client.SellerId}/videos")
            .Add("id", filter.Id).Add("status", filter.Status).Add("page", filter.Page).Add("size", filter.Size).ToString();
        return await RequiredAsync<ProductVideoPage>(
            "Products.GetVideos", HttpMethod.Get, uri,
            "integration/video/sellers/{sellerId}/videos", null, cancellationToken).ConfigureAwait(false);
    }

    public async Task<CreateBrandResponse> CreateBrandAsync(string name, CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotEmpty(name, nameof(name));
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(name), "name");
        return await _client.SendContentAsync<CreateBrandResponse>(
            "Products.CreateBrand", HttpMethod.Post, SellerRoute("brands"),
            "integration/product/sellers/{sellerId}/brands", content, cancellationToken).ConfigureAwait(false)
            ?? throw new JsonException("Trendyol returned an empty brand-create response.");
    }

    private async Task<BatchRequestReference> SendBatchAsync<TItem>(
        string operation,
        HttpMethod method,
        string suffix,
        object? request,
        IReadOnlyCollection<TItem>? items,
        CancellationToken cancellationToken)
    {
        TrendyolGuard.NotNull(request, nameof(request));
        TrendyolGuard.Count(items, "items", 1000);
        return await RequiredAsync<BatchRequestReference>(
            operation, method, SellerRoute(suffix), $"integration/product/sellers/{{sellerId}}/{suffix}",
            request, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ProductPage> GetPageAsync(
        string operation,
        string suffix,
        ProductFilter? filter,
        int maximumSize,
        CancellationToken cancellationToken)
    {
        filter ??= new ProductFilter();
        if (filter.Page < 0 || filter.Size is <= 0 || filter.Size > maximumSize)
        {
            throw new ArgumentOutOfRangeException(nameof(filter), "Page and size are outside the documented range.");
        }

        var uri = new TrendyolQuery(SellerRoute(suffix))
            .Add("barcode", filter.Barcode)
            .AddEpochMilliseconds("startDate", filter.StartDate)
            .AddEpochMilliseconds("endDate", filter.EndDate)
            .Add("page", filter.Page)
            .Add("dateQueryType", filter.DateQueryType)
            .Add("size", filter.Size)
            .Add("stockCode", filter.StockCode)
            .Add("productMainId", filter.ProductMainId)
            .AddCsv("brandIds", filter.BrandIds)
            .Add("status", filter.Status)
            .Add("nextPageToken", filter.NextPageToken)
            .ToString();
        return await RequiredAsync<ProductPage>(
            operation, HttpMethod.Get, uri, $"integration/product/sellers/{{sellerId}}/{suffix}", null, cancellationToken)
            .ConfigureAwait(false);
    }

    private string SellerRoute(string suffix) => $"integration/product/sellers/{_client.SellerId}/{suffix}";

    private async Task<T> RequiredAsync<T>(
        string operation,
        HttpMethod method,
        string uri,
        string routeTemplate,
        object? request,
        CancellationToken cancellationToken) =>
        await _client.SendAsync<T>(operation, method, uri, routeTemplate, request, cancellationToken).ConfigureAwait(false)
        ?? throw new JsonException($"Trendyol returned an empty {operation} response.");
}
