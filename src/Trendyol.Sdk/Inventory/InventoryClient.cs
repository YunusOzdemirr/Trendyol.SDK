using System.Text.Json;
using Trendyol.Sdk.Internal;
using Trendyol.Sdk.Products;

namespace Trendyol.Sdk.Inventory;

internal sealed class InventoryClient : IInventoryClient
{
    private readonly TrendyolClient _client;

    internal InventoryClient(TrendyolClient client) => _client = client;

    public async Task<BatchRequestReference> UpdatePriceAndInventoryAsync(
        PriceAndInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        TrendyolGuard.NotNull(request, nameof(request));
        TrendyolGuard.Count(request.Items, nameof(request.Items), 1000);
        foreach (var item in request.Items)
        {
            TrendyolGuard.NotEmpty(item.Barcode, nameof(item.Barcode));
            if (item.Quantity is < 0 or > 20000)
            {
                throw new ArgumentOutOfRangeException(nameof(request), "Quantity must be between 0 and 20000.");
            }
        }

        var route = $"integration/inventory/sellers/{_client.SellerId}/products/price-and-inventory";
        return await _client.SendAsync<BatchRequestReference>(
            "Inventory.UpdatePriceAndInventory",
            HttpMethod.Post,
            route,
            "integration/inventory/sellers/{sellerId}/products/price-and-inventory",
            request,
            cancellationToken).ConfigureAwait(false)
            ?? throw new JsonException("Trendyol returned an empty inventory batch response.");
    }
}
