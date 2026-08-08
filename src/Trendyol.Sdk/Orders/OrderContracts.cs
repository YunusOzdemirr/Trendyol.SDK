#pragma warning disable CS1591

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Trendyol.Sdk.Orders;

public sealed class ShipmentPackageFilter
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? OrderNumber { get; set; }
    public IReadOnlyCollection<string>? Statuses { get; set; }
    public IReadOnlyCollection<long>? ShipmentPackageIds { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }
    public string? OrderByField { get; set; }
    public string? OrderByDirection { get; set; }
}

public sealed class ShipmentPackageStreamFilter
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? Cursor { get; set; }
    public int? Size { get; set; }
    public IReadOnlyCollection<string>? Statuses { get; set; }
}

public sealed class ShipmentPackagePage
{
    public long TotalElements { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public List<ShipmentPackage> Content { get; set; } = [];
}

public sealed class ShipmentPackageStreamPage
{
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
    public List<ShipmentPackage> Content { get; set; } = [];
}

public sealed class ShipmentPackage
{
    public long Id { get; set; }
    public string? ShipmentPackageStatus { get; set; }
    public string? OrderNumber { get; set; }
    public long? OrderDate { get; set; }
    public long? LastModifiedDate { get; set; }
    public string? CargoTrackingNumber { get; set; }
    public string? CargoTrackingLink { get; set; }
    public long? CargoProviderId { get; set; }
    public long? WarehouseId { get; set; }
    public List<ShipmentPackageLine> Lines { get; set; } = [];

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class ShipmentPackageLine
{
    public long Id { get; set; }
    public int Quantity { get; set; }
    public string? ProductName { get; set; }
    public string? Barcode { get; set; }
    public string? MerchantSku { get; set; }
    public decimal? Price { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public sealed class UpdatePackageStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public List<OrderLineQuantity>? Lines { get; set; }
    public Dictionary<string, string>? Params { get; set; }
}

public sealed class OrderLineQuantity
{
    public long LineId { get; set; }
    public int Quantity { get; set; }
}

public sealed class UnsuppliedItemsRequest
{
    public List<UnsuppliedItem> Lines { get; set; } = [];
}

public sealed class UnsuppliedItem
{
    public long LineId { get; set; }
    public int Quantity { get; set; }
    public long? ReasonId { get; set; }
    public string? Description { get; set; }
}

public sealed class BoxInfoRequest
{
    public decimal PackageVolume { get; set; }
    public int BoxQuantity { get; set; }
}

public sealed class CargoProviderRequest
{
    public long CargoProviderId { get; set; }
}

public sealed class WarehouseRequest
{
    public long WarehouseId { get; set; }
}

public sealed class ExtendedAgreedDeliveryDateRequest
{
    public long ExtendedAgreedDeliveryDate { get; set; }
}

public sealed class LaborCostRequest
{
    public decimal LaborCost { get; set; }
}

public sealed class SplitShipmentPackageRequest
{
    public List<OrderLineQuantity> Lines { get; set; } = [];
}

public sealed class MultiSplitShipmentPackageRequest
{
    public List<SplitShipmentPackageRequest> Packages { get; set; } = [];
}

public sealed class AlternativeDeliveryRequest
{
    public string? TrackingLink { get; set; }
    public string? PhoneNumber { get; set; }
}

public sealed class AlternativeDigitalDeliveryRequest
{
    public List<string> DigitalPinCodes { get; set; } = [];
}
