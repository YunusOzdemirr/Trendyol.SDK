using System.Globalization;
using System.Text;

namespace Trendyol.Sdk.Internal.Http;

internal sealed class TrendyolQuery
{
    private readonly StringBuilder _builder;
    private bool _hasQuery;

    internal TrendyolQuery(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            throw new ArgumentException("A route is required.", nameof(route));
        }

        _builder = new StringBuilder(route);
    }

    internal TrendyolQuery Add(string name, string? value)
    {
        if (value is not null)
        {
            Append(name, value);
        }

        return this;
    }

    internal TrendyolQuery Add(string name, int? value) =>
        Add(name, value?.ToString(CultureInfo.InvariantCulture));

    internal TrendyolQuery Add(string name, long? value) =>
        Add(name, value?.ToString(CultureInfo.InvariantCulture));

    internal TrendyolQuery Add(string name, bool? value) =>
        Add(name, value is null ? null : value.Value ? "true" : "false");

    internal TrendyolQuery AddEpochMilliseconds(string name, DateTimeOffset? value) =>
        Add(name, value?.ToUnixTimeMilliseconds());

    internal TrendyolQuery AddCsv<T>(string name, IEnumerable<T>? values)
    {
        if (values is not null)
        {
            var text = string.Join(",", values.Select(static value =>
                Convert.ToString(value, CultureInfo.InvariantCulture)));
            if (text.Length > 0)
            {
                Append(name, text);
            }
        }

        return this;
    }

    public override string ToString() => _builder.ToString();

    private void Append(string name, string value)
    {
        _builder.Append(_hasQuery ? '&' : '?');
        _hasQuery = true;
        _builder.Append(Uri.EscapeDataString(name));
        _builder.Append('=');
        _builder.Append(Uri.EscapeDataString(value));
    }
}
