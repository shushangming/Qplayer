using System;
using System.Collections.Generic;

namespace AvaloniaLyrics;

public sealed class LyricLine
{
    public string Text { get; init; } = string.Empty;

    public string? Translation { get; init; }

    public string? Romanization { get; init; }

    public TimeSpan Start { get; init; }

    public TimeSpan Duration { get; init; }

    public IReadOnlyList<LyricWord> Words { get; init; } = [];

    public IReadOnlyList<LyricWord> TranslationWords { get; init; } = [];
}
