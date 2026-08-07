namespace Trendyol.Sdk.Configuration;

internal sealed class TrendyolOptionsSnapshot
{
    private static readonly Uri ProductionBaseAddress = new("https://apigw.trendyol.com/", UriKind.Absolute);
    private static readonly Uri StageBaseAddress = new("https://stageapigw.trendyol.com/", UriKind.Absolute);

    private TrendyolOptionsSnapshot(
        long sellerId,
        string apiKey,
        string apiSecret,
        string integratorName,
        TrendyolEnvironment environment,
        TimeSpan timeout)
    {
        SellerId = sellerId;
        ApiKey = apiKey;
        ApiSecret = apiSecret;
        IntegratorName = integratorName;
        Environment = environment;
        Timeout = timeout;
    }

    internal long SellerId { get; }

    internal string ApiKey { get; }

    internal string ApiSecret { get; }

    internal string IntegratorName { get; }

    internal TrendyolEnvironment Environment { get; }

    internal TimeSpan Timeout { get; }

    internal Uri BaseAddress => Environment switch
    {
        TrendyolEnvironment.Production => ProductionBaseAddress,
        TrendyolEnvironment.Stage => StageBaseAddress,
        _ => throw new InvalidOperationException("The Trendyol environment was not validated."),
    };

    internal string UserAgent => $"{SellerId} - {IntegratorName}";

    internal static TrendyolOptionsSnapshot Create(TrendyolOptions options)
    {
#if NET10_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(options);
#else
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }
#endif

        if (options.SellerId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "SellerId must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new ArgumentException("ApiKey must not be empty.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.ApiSecret))
        {
            throw new ArgumentException("ApiSecret must not be empty.", nameof(options));
        }

        if (!IsValidIntegratorName(options.IntegratorName))
        {
            throw new ArgumentException(
                "IntegratorName must contain only ASCII letters and digits and must be between 1 and 30 characters.",
                nameof(options));
        }

        if (!IsDefinedEnvironment(options.Environment))
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Environment must be Production or Stage.");
        }

        if (options.Timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Timeout must be greater than zero.");
        }

        return new TrendyolOptionsSnapshot(
            options.SellerId,
            options.ApiKey,
            options.ApiSecret,
            options.IntegratorName,
            options.Environment,
            options.Timeout);
    }

    internal string Redact(string value)
    {
#if NET10_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(value);
#else
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }
#endif

        return ReplaceOrdinal(ReplaceOrdinal(value, ApiSecret, "[REDACTED]"), ApiKey, "[REDACTED]");
    }

    internal static bool IsValidIntegratorName(string? value)
    {
        if (value is null || value.Length is 0 or > 30)
        {
            return false;
        }

        foreach (var character in value)
        {
            var isAsciiLetter = character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
            var isDigit = character is >= '0' and <= '9';

            if (!isAsciiLetter && !isDigit)
            {
                return false;
            }
        }

        return true;
    }

    internal static bool IsDefinedEnvironment(TrendyolEnvironment environment)
    {
#if NET10_0_OR_GREATER
        return Enum.IsDefined(environment);
#else
        return Enum.IsDefined(typeof(TrendyolEnvironment), environment);
#endif
    }

    private static string ReplaceOrdinal(string value, string oldValue, string newValue)
    {
        var index = value.IndexOf(oldValue, StringComparison.Ordinal);
        if (index < 0)
        {
            return value;
        }

        var result = new System.Text.StringBuilder(value.Length);
        var startIndex = 0;

        while (index >= 0)
        {
            result.Append(value, startIndex, index - startIndex);
            result.Append(newValue);
            startIndex = index + oldValue.Length;
            index = value.IndexOf(oldValue, startIndex, StringComparison.Ordinal);
        }

        result.Append(value, startIndex, value.Length - startIndex);
        return result.ToString();
    }
}
