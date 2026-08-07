namespace Trendyol.Sdk.Catalog;

/// <summary>
/// Provides access to Trendyol catalog operations.
/// </summary>
public interface ICatalogClient
{
    /// <summary>
    /// Searches Trendyol brands using the supplied case-sensitive name filter.
    /// </summary>
    /// <param name="name">The brand name filter. The value is sent without trimming or case normalization.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The brands returned by Trendyol.</returns>
    public Task<IReadOnlyList<Brand>> SearchBrandsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);
}
