using System.Text.Json;
using Trendyol.Sdk.Internal.Http;

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

    public async Task<IReadOnlyList<Brand>> GetBrandsAsync(
        int? page = null,
        int? size = null,
        CancellationToken cancellationToken = default)
    {
        ValidatePage(page, size, 1000);
        var uri = new TrendyolQuery("integration/product/brands")
            .Add("page", page)
            .Add("size", size)
            .ToString();
        var response = await _client.SendAsync<BrandsResponse>(
            "Catalog.GetBrands", HttpMethod.Get, uri, "integration/product/brands", null, cancellationToken)
            .ConfigureAwait(false);

        if (response?.Brands is null)
        {
            throw new JsonException("Trendyol returned an empty brand-list response.");
        }

        return response.Brands.Select(static item =>
            new Brand(item.Id, item.Name ?? throw new JsonException("Trendyol returned a brand without a name."), item.Luxe))
            .ToArray();
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

    public async Task<IReadOnlyList<ProductCategory>> GetCategoryTreeAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await _client.SendAsync<List<ProductCategory>>(
            "Catalog.GetCategoryTree",
            HttpMethod.Get,
            "integration/product/product-categories",
            "integration/product/product-categories",
            null,
            cancellationToken).ConfigureAwait(false);
        return response ?? throw new JsonException("Trendyol returned an empty category-tree response.");
    }

    public async Task<CategoryAttributeSet> GetCategoryAttributesAsync(
        long categoryId,
        CancellationToken cancellationToken = default)
    {
        ValidatePositive(categoryId, nameof(categoryId));
        var route = $"integration/product/categories/{categoryId}/attributes";
        return await _client.SendAsync<CategoryAttributeSet>(
            "Catalog.GetCategoryAttributes",
            HttpMethod.Get,
            route,
            "integration/product/categories/{categoryId}/attributes",
            null,
            cancellationToken).ConfigureAwait(false)
            ?? throw new JsonException("Trendyol returned an empty category-attributes response.");
    }

    public async Task<CategoryAttributeValuePage> GetCategoryAttributeValuesAsync(
        long categoryId,
        long attributeId,
        int? page = null,
        int? size = null,
        CancellationToken cancellationToken = default)
    {
        ValidatePositive(categoryId, nameof(categoryId));
        ValidatePositive(attributeId, nameof(attributeId));
        ValidatePage(page, size, 1000);
        var route = $"integration/product/categories/{categoryId}/attributes/{attributeId}/values";
        var uri = new TrendyolQuery(route).Add("page", page).Add("size", size).ToString();
        return await _client.SendAsync<CategoryAttributeValuePage>(
            "Catalog.GetCategoryAttributeValues",
            HttpMethod.Get,
            uri,
            "integration/product/categories/{categoryId}/attributes/{attributeId}/values",
            null,
            cancellationToken).ConfigureAwait(false)
            ?? throw new JsonException("Trendyol returned an empty category-attribute-values response.");
    }

    private static void ValidatePositive(long value, string name)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(name, "The identifier must be greater than zero.");
        }
    }

    private static void ValidatePage(int? page, int? size, int maximumSize)
    {
        if (page < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page));
        }

        if (size is <= 0 || size > maximumSize)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }
    }

    private sealed class BrandResponse
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public bool? Luxe { get; set; }
    }

    private sealed class BrandsResponse
    {
        public List<BrandResponse>? Brands { get; set; }
    }
}
