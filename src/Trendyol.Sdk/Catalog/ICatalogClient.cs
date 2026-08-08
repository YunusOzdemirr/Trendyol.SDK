#pragma warning disable CS1591

namespace Trendyol.Sdk.Catalog;

/// <summary>
/// Provides access to Trendyol catalog operations.
/// </summary>
public interface ICatalogClient
{
    public Task<IReadOnlyList<Brand>> GetBrandsAsync(
        int? page = null,
        int? size = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches Trendyol brands using the supplied case-sensitive name filter.
    /// </summary>
    /// <param name="name">The brand name filter. The value is sent without trimming or case normalization.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The brands returned by Trendyol.</returns>
    public Task<IReadOnlyList<Brand>> SearchBrandsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    public Task<IReadOnlyList<ProductCategory>> GetCategoryTreeAsync(
        CancellationToken cancellationToken = default);

    public Task<CategoryAttributeSet> GetCategoryAttributesAsync(
        long categoryId,
        CancellationToken cancellationToken = default);

    public Task<CategoryAttributeValuePage> GetCategoryAttributeValuesAsync(
        long categoryId,
        long attributeId,
        int? page = null,
        int? size = null,
        CancellationToken cancellationToken = default);
}
