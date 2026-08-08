#pragma warning disable CS1591

using System.Text.Json;

namespace Trendyol.Sdk.Products;

public sealed class BatchRequestReference
{
    public string BatchRequestId { get; set; } = string.Empty;
}

public sealed class BatchRequestResult
{
    public string BatchRequestId { get; set; } = string.Empty;

    public List<BatchRequestItemResult> Items { get; set; } = [];

    public string Status { get; set; } = string.Empty;

    public long CreationDate { get; set; }

    public long LastModification { get; set; }

    public string SourceType { get; set; } = string.Empty;

    public int ItemCount { get; set; }

    public int FailedItemCount { get; set; }

    public string BatchRequestType { get; set; } = string.Empty;
}

public sealed class BatchRequestItemResult
{
    public JsonElement RequestItem { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<string> FailureReasons { get; set; } = [];
}

public sealed class ProductImage
{
    public string Url { get; set; } = string.Empty;
}

public sealed class ProductAttributeValue
{
    public long AttributeId { get; set; }

    public List<long>? AttributeValueIds { get; set; }

    public string? AttributeValue { get; set; }
}

public sealed class ProductDeliveryOption
{
    public int? DeliveryDuration { get; set; }

    public string? FastDeliveryType { get; set; }
}

public sealed class CreateProductsRequest
{
    public List<CreateProductItem> Items { get; set; } = [];
}

public sealed class CreateProductItem
{
    public string Barcode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ProductMainId { get; set; } = string.Empty;
    public long BrandId { get; set; }
    public long CategoryId { get; set; }
    public int Quantity { get; set; }
    public string StockCode { get; set; } = string.Empty;
    public decimal DimensionalWeight { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal ListPrice { get; set; }
    public decimal SalePrice { get; set; }
    public ProductDeliveryOption? DeliveryOption { get; set; }
    public List<ProductImage> Images { get; set; } = [];
    public int VatRate { get; set; }
    public string? LotNumber { get; set; }
    public long? ShipmentAddressId { get; set; }
    public long? ReturningAddressId { get; set; }
    public List<ProductAttributeValue> Attributes { get; set; } = [];
}

public sealed class UpdateUnapprovedProductsRequest
{
    public List<UpdateUnapprovedProductItem> Items { get; set; } = [];
}

public sealed class UpdateUnapprovedProductItem
{
    public string Barcode { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ProductMainId { get; set; }
    public long? BrandId { get; set; }
    public long? CategoryId { get; set; }
    public string? StockCode { get; set; }
    public decimal? DimensionalWeight { get; set; }
    public int? VatRate { get; set; }
    public ProductDeliveryOption? DeliveryOption { get; set; }
    public string? LocationBasedDelivery { get; set; }
    public string? LotNumber { get; set; }
    public long? ShipmentAddressId { get; set; }
    public long? ReturningAddressId { get; set; }
    public List<ProductImage>? Images { get; set; }
    public List<ProductAttributeValue>? Attributes { get; set; }
}

public sealed class UpdateProductContentRequest
{
    public List<UpdateProductContentItem> Items { get; set; } = [];
}

public sealed class UpdateProductContentItem
{
    public long ContentId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<ProductImage>? Images { get; set; }
    public List<ProductAttributeValue>? Attributes { get; set; }
}

public sealed class UpdateProductVariantsRequest
{
    public List<UpdateProductVariantItem> Items { get; set; } = [];
}

public sealed class UpdateProductVariantItem
{
    public string Barcode { get; set; } = string.Empty;
    public string? StockCode { get; set; }
    public int? VatRate { get; set; }
    public long? ShipmentAddressId { get; set; }
    public long? ReturningAddressId { get; set; }
    public decimal? DimensionalWeight { get; set; }
    public string? LotNumber { get; set; }
    public string? LocationBasedDelivery { get; set; }
}

public sealed class UpdateProductDeliveryRequest
{
    public List<UpdateProductDeliveryItem> Items { get; set; } = [];
}

public sealed class UpdateProductDeliveryItem
{
    public string Barcode { get; set; } = string.Empty;
    public ProductDeliveryOption DeliveryOptions { get; set; } = new();
}

public sealed class ProductFilter
{
    public string? Barcode { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
    public string? DateQueryType { get; set; }
    public string? StockCode { get; set; }
    public string? ProductMainId { get; set; }
    public IReadOnlyCollection<long>? BrandIds { get; set; }
    public string? Status { get; set; }
    public string? NextPageToken { get; set; }
}

public sealed class ProductPage
{
    public long TotalElements { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public string? NextPageToken { get; set; }
    public List<ProductResource> Content { get; set; } = [];
}

public sealed class ProductResource
{
    public long? SupplierId { get; set; }
    public long? ContentId { get; set; }
    public string? ProductMainId { get; set; }
    public string? Barcode { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? Quantity { get; set; }
    public decimal? ListPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public int? VatRate { get; set; }
    public decimal? DimensionalWeight { get; set; }
    public string? StockCode { get; set; }
    public List<ProductImage> Media { get; set; } = [];
    public List<ProductAttributeValue> Attributes { get; set; } = [];
    public List<ProductVariantResource> Variants { get; set; } = [];
}

public sealed class ProductVariantResource
{
    public string? Barcode { get; set; }
    public string? StockCode { get; set; }
    public int? Quantity { get; set; }
    public decimal? ListPrice { get; set; }
    public decimal? SalePrice { get; set; }
}

public sealed class ProductBase
{
    public string Barcode { get; set; } = string.Empty;
    public bool Approved { get; set; }
    public long? ApprovedDate { get; set; }
    public bool Archived { get; set; }
    public string? ListingId { get; set; }
    public long ContentId { get; set; }
}

public sealed class ProductBarcodeRequest
{
    public List<ProductBarcodeItem> Items { get; set; } = [];
}

public sealed class ProductBarcodeItem
{
    public string Barcode { get; set; } = string.Empty;
}

public sealed class ArchiveProductsRequest
{
    public List<ArchiveProductItem> Items { get; set; } = [];
}

public sealed class ArchiveProductItem
{
    public string Barcode { get; set; } = string.Empty;
    public bool Archived { get; set; }
}

public sealed class BuyboxRequest
{
    public List<string> Barcodes { get; set; } = [];
}

public sealed class BuyboxResponse
{
    public List<BuyboxInfo> BuyboxInfo { get; set; } = [];
}

public sealed class BuyboxInfo
{
    public string Barcode { get; set; } = string.Empty;
    public int BuyboxOrder { get; set; }
    public decimal BuyboxPrice { get; set; }
    public bool HasMultipleSeller { get; set; }
}

public sealed class ProductUpdateAudit
{
    public long? Id { get; set; }
    public long? ContentId { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public long? CreatedDate { get; set; }
}

public sealed class CreateProductVideoRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public List<long> ProductContentIds { get; set; } = [];
    public string? VideoContentType { get; set; }
}

public sealed class CreateProductVideoResponse
{
    public string VideoId { get; set; } = string.Empty;
}

public sealed class ProductVideoFilter
{
    public string? Id { get; set; }
    public string? Status { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
}

public sealed class ProductVideoPage
{
    public long TotalElements { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public List<ProductVideo> Content { get; set; } = [];
}

public sealed class ProductVideo
{
    public string Id { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? VideoUrl { get; set; }
    public string? OptimizedVideoUrl { get; set; }
    public string? Status { get; set; }
    public bool IsApproved { get; set; }
    public string? ErrorCode { get; set; }
}

public sealed class CreateBrandResponse
{
    public long? Id { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
}
