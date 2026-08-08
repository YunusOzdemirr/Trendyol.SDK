namespace Trendyol.Sdk.Internal;

internal static class TrendyolGuard
{
    internal static T NotNull<T>(T? value, string name)
        where T : class => value ?? throw new ArgumentNullException(name);

    internal static string NotEmpty(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", name);
        }

        return value!;
    }

    internal static long Positive(long value, string name)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(name, "The value must be greater than zero.");
        }

        return value;
    }

    internal static IReadOnlyCollection<T> Count<T>(IReadOnlyCollection<T>? values, string name, int maximum)
    {
        if (values is null)
        {
            throw new ArgumentNullException(name);
        }

        if (values.Count is 0 || values.Count > maximum)
        {
            throw new ArgumentOutOfRangeException(name, $"The collection must contain between 1 and {maximum} items.");
        }

        return values;
    }
}
