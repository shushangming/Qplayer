using System.Globalization;

namespace AvaloniaSilkEffects.Sonnet;

internal sealed record SonnetLineAnalysis(SonnetLine Line, SonnetTextElement[] Elements,
    List<SonnetToken> Tokens, SonnetGraphemeTiming[] Timeline);

internal static class SonnetSegmentation
{
    public static SonnetLineAnalysis Analyze(SonnetLine line, SonnetLanguage language)
    {
        var elements = SonnetTokenizer.Elements(line.FullText);
        return new SonnetLineAnalysis(line, elements, SonnetTokenizer.Tokenize(line, elements, language), SonnetLyricTimeline.Build(line, elements));
    }

    public static Dictionary<(SonnetLanguage, string), int> CountWords(IEnumerable<SonnetLineAnalysis> lines)
    {
        var counts = new Dictionary<(SonnetLanguage, string), int>();
        foreach (var line in lines)
            foreach (var token in line.Tokens)
                if (token.IsContent)
                {
                    var key = WordKey(token.Word);
                    counts[key] = counts.GetValueOrDefault(key) + 1;
                }
        return counts;
    }

    public static IReadOnlyList<SonnetSemanticSegment> Build(SonnetLineAnalysis analysis,
        IReadOnlyDictionary<(SonnetLanguage, string), int> counts)
    {
        var tokens = analysis.Tokens;
        if (tokens.Count == 0) return [];
        var drafts = Combine(analysis);
        AttachPunctuation(tokens, drafts);
        var output = new List<SonnetSemanticSegment>(drafts.Count);
        foreach (var draft in drafts)
        {
            var from = tokens[draft.Start].Start;
            var to = tokens[draft.End - 1].End;
            var startOffset = analysis.Elements[from].Start;
            var endOffset = analysis.Elements[to - 1].End;
            var timings = analysis.Timeline[from..to];
            var originalWords = tokens.Skip(draft.Start).Take(draft.End - draft.Start)
                .Where(token => token.IsWord).Select(token => token.Word).ToArray();
            SonnetToken? core = draft.Core >= 0 ? tokens[draft.Core] : null;
            var reliableDuration = 0d;
            if (core is not null)
            {
                var reliable = true;
                for (var i = core.Start; i < core.End; i++)
                {
                    var timing = analysis.Timeline[i];
                    reliable &= !timing.IsEstimated;
                    reliableDuration += Math.Max(0, timing.EndTime - timing.StartTime);
                }
                reliableDuration = reliable ? reliableDuration / (core.End - core.Start) : 0;
            }
            output.Add(new SonnetSemanticSegment(analysis.Line.FullText[startOffset..endOffset], startOffset, endOffset,
                timings.Min(item => item.StartTime), timings.Max(item => item.EndTime),
                timings.Where(item => item.WordIndex.HasValue).Select(item => item.WordIndex!.Value).Distinct().ToArray(),
                timings, originalWords.Length > 0)
            {
                LexicalWords = originalWords,
                CoreStartOffset = core?.Word.StartOffset,
                CoreEndOffset = core?.Word.EndOffset,
                Emphasis = new SonnetEmphasis(core is null ? 0 : SonnetFunctionWords.Priority(core.Word.Kind), reliableDuration,
                    core is null ? 0 : counts.GetValueOrDefault(WordKey(core.Word), 1)),
            });
        }
        return output;
    }

    private static (SonnetLanguage, string) WordKey(SonnetLexicalWord word) =>
        (word.Language, word.Language == SonnetLanguage.English ? word.Text.ToUpperInvariant() : word.Text);

    private sealed record Draft(int Start, int End, int Core);
    private readonly record struct Cost(int Orphans, int Merges) : IComparable<Cost>
    {
        public int CompareTo(Cost other) => Orphans != other.Orphans ? Orphans.CompareTo(other.Orphans) : Merges.CompareTo(other.Merges);
    }

    private static List<Draft> Combine(SonnetLineAnalysis analysis)
    {
        var tokens = analysis.Tokens;
        var costs = new Cost[tokens.Count + 1];
        var choices = new Draft[tokens.Count];
        for (var start = tokens.Count - 1; start >= 0; start--)
        {
            var first = tokens[start];
            choices[start] = new Draft(start, start + 1, first.IsContent ? start : -1);
            costs[start] = costs[start + 1] with { Orphans = costs[start + 1].Orphans + (first.Word.Kind == SonnetLexicalKind.Function ? 1 : 0) };
            if (!first.IsWord) continue;
            var wordCount = 0;
            var visible = 0;
            var core = -1;
            var previousWord = -1;
            for (var end = start; end < tokens.Count && end < start + 5; end++)
            {
                var token = tokens[end];
                if (token.Word.Kind == SonnetLexicalKind.Whitespace)
                {
                    if (first.Word.Language != SonnetLanguage.English
                        || token.Word.Text.Any(character => character is '\n' or '\r' or '\v' or '\f' or '\u0085' or '\u2028' or '\u2029')) break;
                    continue;
                }
                if (!token.IsWord || token.Word.Language != first.Word.Language || ++wordCount > 3) break;
                if (previousWord >= 0 && HasPause(tokens[previousWord], token, analysis.Timeline)) break;
                previousWord = end;
                visible += token.End - token.Start;
                if (token.IsContent)
                {
                    if (core >= 0) break;
                    core = end;
                }
                if (wordCount < 2 || core < 0) continue;
                if (first.Word.Language != SonnetLanguage.English && visible > 6) break;
                var allowed = true;
                for (var i = start; i <= end; i++)
                {
                    if (i == core || !tokens[i].IsWord) continue;
                    var direction = SonnetFunctionWords.Attachment(tokens[i].Word.Text, tokens[i].Word.Language);
                    if (direction != (i < core ? SonnetAttachment.Next : SonnetAttachment.Previous)) { allowed = false; break; }
                }
                if (!allowed) continue;
                var candidate = costs[end + 1] with { Merges = costs[end + 1].Merges + 1 };
                if (candidate.CompareTo(costs[start]) >= 0) continue;
                costs[start] = candidate;
                choices[start] = new Draft(start, end + 1, core);
            }
        }
        var result = new List<Draft>();
        for (var index = 0; index < choices.Length; index = choices[index].End) result.Add(choices[index]);
        return result;
    }

    private static bool HasPause(SonnetToken before, SonnetToken after, SonnetGraphemeTiming[] timeline)
    {
        var left = timeline[before.End - 1];
        var right = timeline[after.Start];
        return left.HasReliableEnd && right.HasReliableStart && right.StartTime - left.EndTime >= 0.3 - 1e-9;
    }

    private static void AttachPunctuation(List<SonnetToken> tokens, List<Draft> drafts)
    {
        for (var index = 0; index < drafts.Count;)
        {
            var draft = drafts[index];
            if (draft.End != draft.Start + 1 || tokens[draft.Start].Word.Kind != SonnetLexicalKind.Punctuation)
            { index++; continue; }
            if (IsOpening(tokens, draft.Start))
            {
                var next = index + 1;
                while (next < drafts.Count && drafts[next].End == drafts[next].Start + 1
                    && tokens[drafts[next].Start].Word.Kind == SonnetLexicalKind.Punctuation
                    && IsOpening(tokens, drafts[next].Start)) next++;
                if (next < drafts.Count && ContainsWord(tokens, drafts[next]))
                {
                    drafts[next] = drafts[next] with { Start = draft.Start };
                    drafts.RemoveRange(index, next - index);
                    index++;
                }
                else index++;
            }
            else if (index > 0 && ContainsWord(tokens, drafts[index - 1]))
            {
                drafts[index - 1] = drafts[index - 1] with { End = draft.End };
                drafts.RemoveAt(index);
            }
            else index++;
        }
    }

    private static bool ContainsWord(List<SonnetToken> tokens, Draft draft)
    {
        for (var index = draft.Start; index < draft.End; index++) if (tokens[index].IsWord) return true;
        return false;
    }

    private static bool IsOpening(List<SonnetToken> tokens, int index)
    {
        var text = tokens[index].Word.Text;
        if (text is "\"" or "'")
            return index == 0 || tokens[index - 1].Word.Kind == SonnetLexicalKind.Whitespace
                || System.Text.Rune.GetUnicodeCategory(SonnetTokenizer.FirstRune(tokens[index - 1].Word.Text))
                    is UnicodeCategory.OpenPunctuation or UnicodeCategory.InitialQuotePunctuation;
        return System.Text.Rune.GetUnicodeCategory(SonnetTokenizer.FirstRune(text))
            is UnicodeCategory.OpenPunctuation or UnicodeCategory.InitialQuotePunctuation;
    }
}
