#pragma warning disable CS1591

namespace Trendyol.Sdk.Orders;

public interface IOrdersClient
{
    public Task<ShipmentPackagePage> GetShipmentPackagesAsync(ShipmentPackageFilter? filter = null, CancellationToken cancellationToken = default);
    public Task<ShipmentPackageStreamPage> StreamShipmentPackagesAsync(ShipmentPackageStreamFilter? filter = null, CancellationToken cancellationToken = default);
    public Task UpdatePackageStatusAsync(long packageId, UpdatePackageStatusRequest request, CancellationToken cancellationToken = default);
    public Task MarkItemsUnsuppliedAsync(long packageId, UnsuppliedItemsRequest request, CancellationToken cancellationToken = default);
    public Task UpdateBoxInfoAsync(long packageId, BoxInfoRequest request, CancellationToken cancellationToken = default);
    public Task MarkDeliveredByServiceAsync(long packageId, CancellationToken cancellationToken = default);
    public Task ChangeCargoProviderAsync(long packageId, CargoProviderRequest request, CancellationToken cancellationToken = default);
    public Task UpdateWarehouseAsync(long packageId, WarehouseRequest request, CancellationToken cancellationToken = default);
    public Task ExtendAgreedDeliveryDateAsync(long packageId, ExtendedAgreedDeliveryDateRequest request, CancellationToken cancellationToken = default);
    public Task UpdateLaborCostsAsync(long packageId, LaborCostRequest request, CancellationToken cancellationToken = default);
    public Task SplitPackagesAsync(long packageId, SplitShipmentPackageRequest request, CancellationToken cancellationToken = default);
    public Task SplitAsync(long packageId, SplitShipmentPackageRequest request, CancellationToken cancellationToken = default);
    public Task SplitByQuantityAsync(long packageId, SplitShipmentPackageRequest request, CancellationToken cancellationToken = default);
    public Task MultiSplitAsync(long packageId, MultiSplitShipmentPackageRequest request, CancellationToken cancellationToken = default);
    public Task ProcessAlternativeDeliveryAsync(long packageId, AlternativeDeliveryRequest request, CancellationToken cancellationToken = default);
    public Task ProcessAlternativeDigitalDeliveryAsync(long packageId, AlternativeDigitalDeliveryRequest request, CancellationToken cancellationToken = default);
    public Task MarkDeliveredByPackageIdAsync(long packageId, CancellationToken cancellationToken = default);
    public Task MarkDeliveredByTrackingNumberAsync(string cargoTrackingNumber, CancellationToken cancellationToken = default);
    public Task MarkReturnedByPackageIdAsync(long packageId, CancellationToken cancellationToken = default);
    public Task MarkReturnedByTrackingNumberAsync(string cargoTrackingNumber, CancellationToken cancellationToken = default);
}
