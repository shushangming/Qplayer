using System;

namespace AvaloniaLyrics;

public sealed class LyricWord
{
    public string Text { get; init; } = string.Empty;

    public TimeSpan Start { get; init; }

    public TimeSpan Duration { get; init; }
}
