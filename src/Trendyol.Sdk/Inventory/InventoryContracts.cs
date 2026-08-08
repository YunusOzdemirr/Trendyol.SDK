#pragma warning disable CS1591

namespace Trendyol.Sdk.Inventory;

public sealed class PriceAndInventoryRequest
{
    public List<PriceAndInventoryItem> Items { get; set; } = [];
}

public sealed class PriceAndInventoryItem
{
    public string Barcode { get; set; } = string.Empty;

    public int? Quantity { get; set; }

    public decimal? SalePrice { get; set; }

    public decimal? ListPrice { get; set; }
}
