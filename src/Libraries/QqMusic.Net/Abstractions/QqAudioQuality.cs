namespace QqMusic.Net.Abstractions;

/// <summary>
///     QQ 音乐音质枚举。
/// </summary>
public static class QqAudioQuality
{
    /// <summary>标准品质 128kbps</summary>
    public const string Standard = "128";

    /// <summary>高品质 320kbps</summary>
    public const string High = "320";

    /// <summary>无损 FLAC</summary>
    public const string Flac = "flac";

    /// <summary>AAC 格式</summary>
    public const string M4a = "m4a";

    /// <summary>APE 无损</summary>
    public const string Apex = "ape";

    /// <summary>默认音质</summary>
    public const string Default = Standard;
}
