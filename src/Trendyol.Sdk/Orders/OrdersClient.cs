using System.Text.Json;
using Trendyol.Sdk.Internal;
using Trendyol.Sdk.Internal.Http;

namespace Trendyol.Sdk.Orders;

internal sealed class OrdersClient : IOrdersClient
{
    private readonly TrendyolClient _client;

    internal OrdersClient(TrendyolClient client) => _client = client;

    public Task<ShipmentPackagePage> GetShipmentPackagesAsync(ShipmentPackageFilter? filter = null, CancellationToken cancellationToken = default)
    {
        filter ??= new ShipmentPackageFilter();
        ValidatePage(filter.Page, filter.Size);
        var uri = new TrendyolQuery(SellerRoute("orders"))
            .AddEpochMilliseconds("startDate", filter.StartDate).AddEpochMilliseconds("endDate", filter.EndDate)
            .Add("orderNumber", filter.OrderNumber).AddCsv("status", filter.Statuses)
            .AddCsv("shipmentPackageIds", filter.ShipmentPackageIds).Add("page", filter.Page).Add("size", filter.Size)
            .Add("orderByField", filter.OrderByField).Add("orderByDirection", filter.OrderByDirection).ToString();
        return RequiredAsync<ShipmentPackagePage>("Orders.GetShipmentPackages", HttpMethod.Get, uri, "integration/order/sellers/{sellerId}/orders", null, cancellationToken);
    }

    public Task<ShipmentPackageStreamPage> StreamShipmentPackagesAsync(ShipmentPackageStreamFilter? filter = null, CancellationToken cancellationToken = default)
    {
        filter ??= new ShipmentPackageStreamFilter();
        ValidatePage(null, filter.Size);
        var uri = new TrendyolQuery(SellerRoute("orders/stream"))
            .AddEpochMilliseconds("startDate", filter.StartDate).AddEpochMilliseconds("endDate", filter.EndDate)
            .Add("cursor", filter.Cursor).Add("size", filter.Size).AddCsv("status", filter.Statuses).ToString();
        return RequiredAsync<ShipmentPackageStreamPage>("Orders.StreamShipmentPackages", HttpMethod.Get, uri, "integration/order/sellers/{sellerId}/orders/stream", null, cancellationToken);
    }

    public Task UpdatePackageStatusAsync(long packageId, UpdatePackageStatusRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync("UpdatePackageStatus", packageId, string.Empty, request, cancellationToken);
    public Task MarkItemsUnsuppliedAsync(long packageId, UnsuppliedItemsRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync("MarkItemsUnsupplied", packageId, "items/unsupplied", request, cancellationToken);
    public Task UpdateBoxInfoAsync(long packageId, BoxInfoRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync("UpdateBoxInfo", packageId, "box-info", request, cancellationToken);
    public Task MarkDeliveredByServiceAsync(long packageId, CancellationToken cancellationToken = default) =>
        MutateAsync("MarkDeliveredByService", packageId, "delivered-by-service", null, cancellationToken);
    public Task ChangeCargoProviderAsync(long packageId, CargoProviderRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync("ChangeCargoProvider", packageId, "cargo-providers", request, cancellationToken);
    public Task UpdateWarehouseAsync(long packageId, WarehouseRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync("UpdateWarehouse", packageId, "warehouse", request, cancellationToken);
    public Task ExtendAgreedDeliveryDateAsync(long packageId, ExtendedAgreedDeliveryDateRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync("ExtendAgreedDeliveryDate", packageId, "extended-agreed-delivery-date", request, cancellationToken);
    public Task UpdateLaborCostsAsync(long packageId, LaborCostRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync("UpdateLaborCosts", packageId, "labor-costs", request, cancellationToken);
    public Task SplitPackagesAsync(long packageId, SplitShipmentPackageRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync("SplitPackages", packageId, "split-packages", request, cancellationToken, HttpMethod.Post);
    public Task SplitAsync(long packageId, SplitShipmentPackageRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync("Split", packageId, "split", request, cancellationToken, HttpMethod.Post);
    public Task SplitByQuantityAsync(long packageId, SplitShipmentPackageRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync("SplitByQuantity", packageId, "quantity-split", request, cancellationToken, HttpMethod.Post);
    public Task MultiSplitAsync(long packageId, MultiSplitShipmentPackageRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync("MultiSplit", packageId, "multi-split", request, cancellationToken, HttpMethod.Post);
    public Task ProcessAlternativeDeliveryAsync(long packageId, AlternativeDeliveryRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync("ProcessAlternativeDelivery", packageId, "alternative-delivery", request, cancellationToken);
    public Task ProcessAlternativeDigitalDeliveryAsync(long packageId, AlternativeDigitalDeliveryRequest request, CancellationToken cancellationToken = default) =>
        MutateAsync("ProcessAlternativeDigitalDelivery", packageId, "alternative-delivery-digital", request, cancellationToken);
    public Task MarkDeliveredByPackageIdAsync(long packageId, CancellationToken cancellationToken = default) =>
        MutateAsync("MarkDeliveredByPackageId", packageId, "manual-invoice-delivery", null, cancellationToken);
    public Task MarkReturnedByPackageIdAsync(long packageId, CancellationToken cancellationToken = default) =>
        MutateAsync("MarkReturnedByPackageId", packageId, "manual-return", null, cancellationToken);

    public Task MarkDeliveredByTrackingNumberAsync(string cargoTrackingNumber, CancellationToken cancellationToken = default) =>
        MutateByTrackingAsync("MarkDeliveredByTrackingNumber", "manual-invoice-delivery-by-tracking-number", cargoTrackingNumber, cancellationToken);
    public Task MarkReturnedByTrackingNumberAsync(string cargoTrackingNumber, CancellationToken cancellationToken = default) =>
        MutateByTrackingAsync("MarkReturnedByTrackingNumber", "manual-return-by-tracking-number", cargoTrackingNumber, cancellationToken);

    private Task MutateAsync(string operation, long packageId, string suffix, object? request, CancellationToken cancellationToken, HttpMethod? method = null)
    {
        TrendyolGuard.Positive(packageId, nameof(packageId));
        if (request is not null)
        {
            TrendyolGuard.NotNull(request, nameof(request));
        }

        var tail = string.IsNullOrEmpty(suffix) ? string.Empty : $"/{suffix}";
        var uri = SellerRoute($"shipment-packages/{packageId}{tail}");
        return _client.SendAsync($"Orders.{operation}", method ?? HttpMethod.Put, uri,
            $"integration/order/sellers/{{sellerId}}/shipment-packages/{{packageId}}{tail}", request, cancellationToken);
    }

    private Task MutateByTrackingAsync(string operation, string suffix, string trackingNumber, CancellationToken cancellationToken)
    {
        TrendyolGuard.NotEmpty(trackingNumber, nameof(trackingNumber));
        var uri = SellerRoute($"shipment-packages/{suffix}/{Uri.EscapeDataString(trackingNumber)}");
        return _client.SendAsync($"Orders.{operation}", HttpMethod.Put, uri,
            $"integration/order/sellers/{{sellerId}}/shipment-packages/{suffix}/{{cargoTrackingNumber}}", null, cancellationToken);
    }

    private string SellerRoute(string suffix) => $"integration/order/sellers/{_client.SellerId}/{suffix}";

    private async Task<T> RequiredAsync<T>(string operation, HttpMethod method, string uri, string template, object? request, CancellationToken cancellationToken) =>
        await _client.SendAsync<T>(operation, method, uri, template, request, cancellationToken).ConfigureAwait(false)
        ?? throw new JsonException($"Trendyol returned an empty {operation} response.");

    private static void ValidatePage(int? page, int? size)
    {
        if (page < 0 || size is <= 0 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Page and size are outside the documented range.");
        }
    }
}
