namespace KuGou.Net.Abstractions;

public static class AudioQuality
{
    public const string Standard = "128";
    public const string High = "320";
    public const string Lossless = "flac";
    public const string HiRes = "high";
    public const string Default = Standard;

    private static readonly string[] OrderedValues = [Standard, High, Lossless, HiRes];

    public static IReadOnlyList<string> Ordered => OrderedValues;

    public static string Normalize(string? quality)
    {
        return quality?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    public static int GetRank(string? quality)
    {
        var normalizedQuality = Normalize(quality);
        return Array.FindIndex(OrderedValues,
            candidate => string.Equals(candidate, normalizedQuality, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsKnown(string? quality)
    {
        return GetRank(quality) >= 0;
    }

    public static string GetNext(string? quality)
    {
        var rank = GetRank(quality);
        return OrderedValues[(rank + 1) % OrderedValues.Length];
    }
}
