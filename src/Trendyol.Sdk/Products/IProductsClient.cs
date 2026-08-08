#pragma warning disable CS1591

namespace Trendyol.Sdk.Products;

public interface IProductsClient
{
    public Task<BatchRequestReference> CreateAsync(CreateProductsRequest request, CancellationToken cancellationToken = default);
    public Task<BatchRequestReference> UpdateUnapprovedAsync(UpdateUnapprovedProductsRequest request, CancellationToken cancellationToken = default);
    public Task<BatchRequestReference> UpdateContentAsync(UpdateProductContentRequest request, CancellationToken cancellationToken = default);
    public Task<BatchRequestReference> UpdateVariantsAsync(UpdateProductVariantsRequest request, CancellationToken cancellationToken = default);
    public Task<BatchRequestReference> UpdateDeliveryAsync(UpdateProductDeliveryRequest request, CancellationToken cancellationToken = default);
    public Task<ProductBase> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    public Task<ProductPage> GetUnapprovedAsync(ProductFilter? filter = null, CancellationToken cancellationToken = default);
    public Task<ProductPage> GetApprovedAsync(ProductFilter? filter = null, CancellationToken cancellationToken = default);
    public Task<ProductPage> GetApprovedInventoryAndPriceAsync(ProductFilter? filter = null, CancellationToken cancellationToken = default);
    public Task<BatchRequestReference> DeleteAsync(ProductBarcodeRequest request, CancellationToken cancellationToken = default);
    public Task<BatchRequestReference> SetArchiveStateAsync(ArchiveProductsRequest request, CancellationToken cancellationToken = default);
    public Task<BatchRequestReference> UnlockAsync(ProductBarcodeRequest request, CancellationToken cancellationToken = default);
    public Task<BuyboxResponse> GetBuyboxInformationAsync(BuyboxRequest request, CancellationToken cancellationToken = default);
    public Task<BatchRequestResult> GetBatchResultAsync(string batchRequestId, CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<ProductUpdateAudit>> GetUpdateAuditsAsync(long contentId, CancellationToken cancellationToken = default);
    public Task<CreateProductVideoResponse> CreateVideoAsync(CreateProductVideoRequest request, CancellationToken cancellationToken = default);
    public Task<ProductVideoPage> GetVideosAsync(ProductVideoFilter? filter = null, CancellationToken cancellationToken = default);
    public Task<CreateBrandResponse> CreateBrandAsync(string name, CancellationToken cancellationToken = default);
}
