namespace Trendyol.Sdk.Catalog;

/// <summary>
/// Represents a brand in the Trendyol product catalog.
/// </summary>
public sealed class Brand
{
    /// <summary>
    /// Initializes a brand value.
    /// </summary>
    /// <param name="id">The Trendyol brand identifier.</param>
    /// <param name="name">The brand name.</param>
    /// <param name="luxe">
    /// The optional value of the Trendyol <c>luxe</c> response field.
    /// </param>
    public Brand(long id, string name, bool? luxe = null)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Luxe = luxe;
    }

    /// <summary>
    /// Gets the Trendyol brand identifier.
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// Gets the brand name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the optional value of the Trendyol <c>luxe</c> response field.
    /// </summary>
    /// <remarks>
    /// The field is nullable because it is present in some official response examples and absent from others.
    /// </remarks>
    public bool? Luxe { get; }
}
