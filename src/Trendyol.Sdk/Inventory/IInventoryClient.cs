#pragma warning disable CS1591

using Trendyol.Sdk.Products;

namespace Trendyol.Sdk.Inventory;

public interface IInventoryClient
{
    public Task<BatchRequestReference> UpdatePriceAndInventoryAsync(
        PriceAndInventoryRequest request,
        CancellationToken cancellationToken = default);
}
