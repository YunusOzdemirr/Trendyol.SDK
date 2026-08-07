using System.Text.Json;

namespace Trendyol.Sdk.Catalog;

internal sealed class CatalogClient : ICatalogClient
{
    private const string BrandSearchOperation = "Catalog.SearchBrandsByName";
    private const string BrandSearchRoute = "integration/product/brands/by-name";

    private readonly TrendyolClient _client;

    internal CatalogClient(TrendyolClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<IReadOnlyList<Brand>> SearchBrandsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
#if NET10_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(name);
#else
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }
#endif

        var relativeUri = $"{BrandSearchRoute}?name={Uri.EscapeDataString(name)}";
        var response = await _client.SendAsync<List<BrandResponse?>>(
            BrandSearchOperation,
            HttpMethod.Get,
            relativeUri,
            BrandSearchRoute,
            requestBody: null,
            cancellationToken).ConfigureAwait(false);

        if (response is null)
        {
            throw new JsonException("Trendyol returned an empty brand-search response.");
        }

        var brands = new Brand[response.Count];
        for (var index = 0; index < response.Count; index++)
        {
            var item = response[index];
            if (item?.Name is null)
            {
                throw new JsonException("Trendyol returned a brand without a name.");
            }

            brands[index] = new Brand(item.Id, item.Name, item.Luxe);
        }

        return brands;
    }

    private sealed class BrandResponse
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public bool? Luxe { get; set; }
    }
}
