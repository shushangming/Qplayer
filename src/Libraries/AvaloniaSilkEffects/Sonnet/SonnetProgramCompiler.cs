namespace AvaloniaSilkEffects.Sonnet;

public static partial class SonnetProgramCompiler
{
    public static readonly IReadOnlyList<SonnetShotKind> ShotKinds =
    [
        SonnetShotKind.EditorialColumn, SonnetShotKind.TypeImpact,
        SonnetShotKind.FragmentCollage, SonnetShotKind.TrackingRibbon,
        SonnetShotKind.MaskReveal, SonnetShotKind.PosterBlocks,
        SonnetShotKind.QuietTableau,
    ];

    private static readonly SonnetTransitionKind[] TransitionKinds =
        [SonnetTransitionKind.FastBlur, SonnetTransitionKind.MonoGlitch, SonnetTransitionKind.CameraPull];

    public static SonnetProgram Compile(IReadOnlyList<SonnetLine> source, string seed = "sonnet")
    {
        var language = SonnetTokenizer.SongLanguage(source);
        var analyses = source.Select(line => SonnetSegmentation.Analyze(line, language)).ToArray();
        var wordCounts = SonnetSegmentation.CountWords(analyses);
        var lines = source.Select((line, index) => new SonnetCompiledLine(
            index, line,
            Math.Max(line.StartTime, Math.Min(line.RenderEndTime ?? line.EndTime,
                index + 1 < source.Count ? source[index + 1].StartTime : double.PositiveInfinity)),
            SonnetSegmentation.Build(analyses[index], wordCounts))).ToArray();
        if (lines.Length == 0)
            return new SonnetProgram(seed, 1.25, []);

        var threshold = ResolveParagraphGapThreshold(source);
        var drafts = SplitParagraphs(lines, threshold);
        SonnetShotKind? previousShot = null;
        SonnetTransitionKind? previousTransition = null;
        var paragraphs = new List<SonnetParagraph>(drafts.Count);

        for (var index = 0; index < drafts.Count; index++)
        {
            var draft = drafts[index];
            var kind = ClassifyParagraph(draft.Lines, index, drafts.Count);
            var shots = BuildShots(draft.Lines, kind, index, seed, ref previousShot);
            var endTime = draft.Lines[^1].RenderEndTime;
            SonnetTransition? transition = null;
            if (index + 1 < drafts.Count)
            {
                var nextStart = drafts[index + 1].Lines[0].Line.StartTime;
                var transitionKind = ChooseWithoutRepeat(TransitionKinds, $"{seed}:{index}:transition", previousTransition);
                previousTransition = transitionKind;
                var gap = nextStart - endTime;
                var duration = Math.Min(0.3, Math.Max(0.16, gap > 0 ? gap * 0.5 : 0.2));
                transition = new SonnetTransition(transitionKind, Math.Max(draft.Lines[0].Line.StartTime, nextStart - duration), nextStart);
            }

            paragraphs.Add(new SonnetParagraph(
                $"sonnet-p{index}", kind, draft.Boundary,
                draft.Lines[0].Line.StartTime, endTime, draft.Lines, shots, transition));
        }

        return new SonnetProgram(seed, threshold, paragraphs);
    }

    public static int FindParagraphIndexAtTime(SonnetProgram program, double time)
    {
        for (var index = program.Paragraphs.Count - 1; index >= 0; index--)
            if (time >= program.Paragraphs[index].StartTime)
                return index;
        return 0;
    }

    public static double ResolveParagraphGapThreshold(IReadOnlyList<SonnetLine> lines)
    {
        var gaps = lines.Skip(1).Select((line, index) =>
            line.StartTime - Math.Min(lines[index].RenderEndTime ?? lines[index].EndTime, line.StartTime))
            .Where(gap => gap > 0).Order().ToArray();
        var median = gaps.Length == 0 ? 0.5 : gaps.Length % 2 == 0
            ? (gaps[gaps.Length / 2 - 1] + gaps[gaps.Length / 2]) / 2
            : gaps[gaps.Length / 2];
        return Math.Clamp(median * 2.5, 1.25, 3.5);
    }

    public static IReadOnlyList<SonnetSemanticSegment> BuildSemanticSegments(SonnetLine line)
    {
        var analysis = SonnetSegmentation.Analyze(line, SonnetLanguage.Chinese);
        return SonnetSegmentation.Build(analysis, SonnetSegmentation.CountWords([analysis]));
    }

    private static List<ParagraphDraft> SplitParagraphs(SonnetCompiledLine[] lines, double threshold)
    {
        var initial = new List<ParagraphDraft>();
        var current = new List<SonnetCompiledLine>();
        var boundary = SonnetParagraphBoundary.SongStart;
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            if (index > 0)
            {
                var previous = lines[index - 1];
                var metadata = previous.Line.BlockIndex.HasValue && line.Line.BlockIndex.HasValue &&
                    previous.Line.BlockIndex != line.Line.BlockIndex ||
                    previous.Line.SongPart is not null && line.Line.SongPart is not null && previous.Line.SongPart != line.Line.SongPart;
                var nextBoundary = metadata ? SonnetParagraphBoundary.Metadata :
                    line.Line.StartTime - previous.RenderEndTime >= threshold ? SonnetParagraphBoundary.TimeGap : (SonnetParagraphBoundary?)null;
                if (nextBoundary.HasValue && current.Count > 0)
                {
                    initial.Add(new ParagraphDraft(current.ToArray(), boundary));
                    current.Clear();
                    boundary = nextBoundary.Value;
                }
            }
            current.Add(line);
        }
        if (current.Count > 0) initial.Add(new ParagraphDraft(current.ToArray(), boundary));

        var output = new List<ParagraphDraft>();
        foreach (var draft in initial)
        {
            var remaining = draft.Lines.ToList();
            var draftBoundary = draft.Boundary;
            while (remaining.Count > 6 || remaining.Count > 1 && remaining[^1].RenderEndTime - remaining[0].Line.StartTime > 18)
            {
                var split = Math.Min(4, remaining.Count - 1);
                var candidates = Enumerable.Range(2, Math.Max(0, remaining.Count - 3))
                    .Select(i => (Index: i, Gap: remaining[i].Line.StartTime - remaining[i - 1].RenderEndTime))
                    .OrderByDescending(item => item.Gap).ToArray();
                if (candidates.Length > 0) split = candidates[0].Index;
                output.Add(new ParagraphDraft(remaining.Take(split).ToArray(), draftBoundary));
                remaining = remaining.Skip(split).ToList();
                draftBoundary = split >= 6 ? SonnetParagraphBoundary.LineCap : SonnetParagraphBoundary.DurationCap;
            }
            output.Add(new ParagraphDraft(remaining, draftBoundary));
        }
        return output;
    }

    private static SonnetParagraphKind ClassifyParagraph(IReadOnlyList<SonnetCompiledLine> lines, int index, int total)
    {
        if (lines.Any(item => item.Line.IsChorus || Contains(item.Line.SongPart, "chorus", "副歌"))) return SonnetParagraphKind.Chorus;
        if (lines.Any(item => Contains(item.Line.SongPart, "bridge", "break", "間奏", "ブリッジ"))) return SonnetParagraphKind.Break;
        if (index == total - 1) return SonnetParagraphKind.Outro;
        var duration = lines[^1].RenderEndTime - lines[0].Line.StartTime;
        var words = lines.Sum(item => item.Segments.Sum(segment => segment.LexicalWords.Count));
        var punctuation = lines.Sum(item => item.Line.FullText.Count(character => "!?！？…".Contains(character)));
        if (duration <= 3.5 || words <= 3) return SonnetParagraphKind.Breath;
        if (punctuation >= 2 || words / Math.Max(duration, 1) > 2.5) return SonnetParagraphKind.Lift;
        return SonnetParagraphKind.Verse;
    }

    private static IReadOnlyList<SonnetShot> BuildShots(
        IReadOnlyList<SonnetCompiledLine> lines, SonnetParagraphKind paragraphKind,
        int paragraphIndex, string seed, ref SonnetShotKind? previous)
    {
        var groups = new List<List<SonnetCompiledLine>>();
        foreach (var line in lines)
        {
            if (groups.Count == 0 || groups[^1].Count >= 4 || line.RenderEndTime - groups[^1][0].Line.StartTime > 6)
                groups.Add([]);
            groups[^1].Add(line);
        }
        var shots = new List<SonnetShot>();
        for (var shotIndex = 0; shotIndex < groups.Count; shotIndex++)
        {
            var group = groups[shotIndex];
            var signature = string.Join('|', group.Select(item => item.Line.FullText));
            var kind = ChooseWithoutRepeat(ShotKinds, $"{seed}:{paragraphIndex}:{shotIndex}:{signature}", previous);
            var wordCount = group.Sum(item => item.Segments.Sum(segment => segment.LexicalWords.Count));
            if (paragraphKind == SonnetParagraphKind.Breath && shotIndex == 0 && wordCount <= 2) kind = SonnetShotKind.QuietTableau;
            if (paragraphKind == SonnetParagraphKind.Chorus && kind == SonnetShotKind.QuietTableau) kind = SonnetShotKind.TypeImpact;
            previous = kind;
            var random = SonnetRandom.Hash($"{seed}:{paragraphIndex}:{shotIndex}:camera");
            var zoomRandom = (random >> 16 & 255) / 255d;
            var (zoomBase, zoomSpan) = kind switch
            {
                SonnetShotKind.PosterBlocks => (1.02, 0.16),
                SonnetShotKind.QuietTableau => (1.12, 0.2),
                _ => (1.22, 0.26),
            };
            var segments = group.SelectMany(item => item.Segments).Where(item => item.Text.Length > 0).ToArray();
            var cues = segments.Select((segment, i) => new SonnetAnimationCue(
                segment.StartTime, Math.Max(0.08, segment.EndTime - segment.StartTime),
                i == segments.Length - 1 ? "accent" : "enter", i, i + 1)).ToArray();
            shots.Add(new SonnetShot(
                $"p{paragraphIndex}-s{shotIndex}", kind, group[0].Line.StartTime, group[^1].RenderEndTime,
                group.Select(item => item.SourceIndex).ToArray(), cues,
                new SonnetCamera((random & 255) / 255d * 0.18 - 0.09, (random >> 8 & 255) / 255d * 0.14 - 0.07,
                    zoomBase + zoomRandom * zoomSpan, ((random >> 24 & 255) / 255d - 0.5) * 0.08)));
        }
        return shots;
    }

    private static T ChooseWithoutRepeat<T>(IReadOnlyList<T> choices, string seed, T? previous) where T : struct
    {
        var start = (int)(SonnetRandom.Hash(seed) % choices.Count);
        for (var offset = 0; offset < choices.Count; offset++)
        {
            var candidate = choices[(start + offset) % choices.Count];
            if (!previous.HasValue || !EqualityComparer<T>.Default.Equals(candidate, previous.Value)) return candidate;
        }
        return choices[start];
    }

    private static bool Contains(string? value, params string[] candidates) => value is not null &&
        candidates.Any(candidate => value.Contains(candidate, StringComparison.OrdinalIgnoreCase));

    private sealed record ParagraphDraft(IReadOnlyList<SonnetCompiledLine> Lines, SonnetParagraphBoundary Boundary);
}
