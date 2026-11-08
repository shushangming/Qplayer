namespace AvaloniaSilkEffects.Sonnet;

internal static class SonnetLyricTimeline
{
    public static SonnetGraphemeTiming[] Build(SonnetLine line, SonnetTextElement[] elements)
    {
        var result = new SonnetGraphemeTiming?[elements.Length];
        var startTime = double.IsFinite(line.StartTime) ? Math.Max(0, line.StartTime) : 0;
        var endTime = double.IsFinite(line.EndTime) ? Math.Max(startTime, line.EndTime) : startTime;
        var offsetToElement = new Dictionary<int, int>(elements.Length + 1);
        for (var i = 0; i < elements.Length; i++) offsetToElement.Add(elements[i].Start, i);
        offsetToElement[line.FullText.Length] = elements.Length;
        var cursor = 0;
        for (var wordIndex = 0; wordIndex < line.Words.Count && cursor < elements.Length; wordIndex++)
        {
            var word = line.Words[wordIndex];
            if (string.IsNullOrEmpty(word.Text) || !double.IsFinite(word.StartTime) || !double.IsFinite(word.EndTime)
                || word.EndTime < word.StartTime) continue;
            var searchOffset = elements[cursor].Start;
            var from = -1;
            var to = -1;
            while (searchOffset < line.FullText.Length)
            {
                var match = line.FullText.IndexOf(word.Text, searchOffset, StringComparison.Ordinal);
                if (match < 0) break;
                if (offsetToElement.TryGetValue(match, out from)
                    && offsetToElement.TryGetValue(match + word.Text.Length, out to)) break;
                from = to = -1;
                searchOffset = match + 1;
            }
            // A failed tag consumes neither original text nor another tag's timing.
            if (from < cursor || to <= from) continue;
            var wordStart = Math.Clamp(word.StartTime, startTime, endTime);
            var wordEnd = Math.Clamp(word.EndTime, wordStart, endTime);
            var count = to - from;
            var adjusted = wordStart != word.StartTime || wordEnd != word.EndTime;
            for (var i = from; i < to; i++)
                result[i] = new SonnetGraphemeTiming(elements[i].Text,
                    wordStart + (wordEnd - wordStart) * (i - from) / count,
                    i + 1 == to ? wordEnd : wordStart + (wordEnd - wordStart) * (i - from + 1) / count,
                    wordIndex)
                {
                    IsEstimated = count > 1 || adjusted,
                    HasReliableStart = i == from && !adjusted,
                    HasReliableEnd = i + 1 == to && !adjusted,
                };
            cursor = to;
        }
        for (var index = 0; index < result.Length;)
        {
            if (result[index] is not null) { index++; continue; }
            var from = index;
            while (index < result.Length && result[index] is null) index++;
            var gapStart = from > 0 ? result[from - 1]!.EndTime : startTime;
            var gapEnd = index < result.Length ? result[index]!.StartTime : endTime;
            // Adjacent sung words may legitimately overlap. Keep their original
            // anchors and put untimed text at the next anchor instead of moving
            // either word or inventing a negative-duration interpolation.
            gapStart = Math.Min(gapStart, gapEnd);
            for (var i = from; i < index; i++)
                result[i] = new SonnetGraphemeTiming(elements[i].Text,
                    gapStart + (gapEnd - gapStart) * (i - from) / (index - from),
                    gapStart + (gapEnd - gapStart) * (i - from + 1) / (index - from))
                { IsEstimated = true, HasReliableStart = false, HasReliableEnd = false };
        }
        return Array.ConvertAll(result, timing => timing!);
    }
}
