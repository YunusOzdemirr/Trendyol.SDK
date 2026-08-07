using System.Text.Json;
using Trendyol.Sdk.Internal.Serialization;

namespace Trendyol.Sdk.UnitTests.Internal;

public sealed class TrendyolJsonTests
{
    [Fact]
    public void SerializerUsesCamelCaseAndOmitsNulls()
    {
        var value = new SerializationContract
        {
            RequiredValue = "value",
            OptionalValue = null,
        };

        var json = JsonSerializer.Serialize(value, TrendyolJson.Options);

        Assert.Equal("{\"requiredValue\":\"value\"}", json);
        Assert.True(TrendyolJson.Options.IsReadOnly);
    }

    [Fact]
    public void DeserializerIsCaseInsensitiveAndIgnoresUnknownFields()
    {
        const string json = "{\"REQUIREDVALUE\":\"value\",\"newServerField\":42}";

        var result = JsonSerializer.Deserialize<SerializationContract>(json, TrendyolJson.Options);

        Assert.NotNull(result);
        Assert.Equal("value", result.RequiredValue);
    }

    private sealed class SerializationContract
    {
        public string RequiredValue { get; set; } = string.Empty;

        public string? OptionalValue { get; set; }
    }
}
