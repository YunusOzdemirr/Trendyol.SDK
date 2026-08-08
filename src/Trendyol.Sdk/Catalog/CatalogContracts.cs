#pragma warning disable CS1591

namespace Trendyol.Sdk.Catalog;

public sealed class ProductCategory
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public long? ParentId { get; set; }

    public List<ProductCategory> SubCategories { get; set; } = [];
}

public sealed class CategoryAttributeDefinition
{
    public bool AllowCustom { get; set; }

    public CategoryAttributeInfo Attribute { get; set; } = new();

    public long CategoryId { get; set; }

    public bool Required { get; set; }

    public bool Varianter { get; set; }

    public bool Slicer { get; set; }

    public bool AllowMultipleAttributeValues { get; set; }
}

public sealed class CategoryAttributeInfo
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public sealed class CategoryAttributeSet
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public List<CategoryAttributeDefinition> CategoryAttributes { get; set; } = [];
}

public sealed class CategoryAttributeValue
{
    public long AttributeValueId { get; set; }

    public string AttributeValueName { get; set; } = string.Empty;
}

public sealed class CategoryAttributeValuePage
{
    public long TotalElements { get; set; }

    public int TotalPages { get; set; }

    public int Page { get; set; }

    public int Size { get; set; }

    public List<CategoryAttributeValue> Content { get; set; } = [];
}
