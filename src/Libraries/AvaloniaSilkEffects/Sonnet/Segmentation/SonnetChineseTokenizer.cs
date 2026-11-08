using System.Text;

namespace AvaloniaSilkEffects.Sonnet;

// Jieba's dictionary DAG / maximum-probability route and BMES unknown-word model.
// See LanguageData for the pinned upstream revision, data hashes and MIT license.
internal static class SonnetChineseTokenizer
{
    public static void Append(string text, SonnetTextElement[] elements, int start, int end, List<SonnetToken> output)
    {
        var lexicon = SonnetChineseLexicon.Instance;
        var count = end - start;
        var scores = new double[count + 1];
        var next = new int[count];
        var kinds = new SonnetLexicalKind[count];
        var known = new bool[count];
        Span<byte> prefixBuffer = stackalloc byte[lexicon.MaxUtf8Length];
        Span<byte> wordBuffer = stackalloc byte[lexicon.MaxUtf8Length];
        for (var index = count - 1; index >= 0; index--)
        {
            // Always keep the original unknown-single-grapheme edge, including
            // when even the first grapheme has no dictionary prefix.
            scores[index] = lexicon.UnknownScore + scores[index + 1];
            next[index] = index + 1;
            var scanner = lexicon.Scan(prefixBuffer, wordBuffer);
            for (var finish = index + 1; finish <= count; finish++)
            {
                if (!scanner.Advance(elements[start + finish - 1].Text.AsSpan(), out var found, out var cost, out var kind)) break;
                if (!found) continue;
                var score = cost + scores[finish];
                // A known single replaces its fallback even if float storage
                // rounds its probability slightly below UnknownScore.
                if (finish != index + 1 && score < scores[index]) continue;
                scores[index] = score;
                next[index] = finish;
                kinds[index] = kind;
                known[index] = true;
            }
        }
        for (var index = 0; index < count;)
        {
            if (next[index] > index + 1)
            {
                Add(index, next[index], kinds[index], SonnetLexicalSource.Dictionary);
                index = next[index];
                continue;
            }
            var finish = index + 1;
            while (finish < count && next[finish] == finish + 1) finish++;
            var from = elements[start + index].Start;
            var to = elements[start + finish - 1].End;
            if (finish - index > 1 && !lexicon.TryGet(text.AsSpan(from, to - from), out _, out _))
            {
                var boundaries = SonnetChineseHmm.Instance.Split(elements, start + index, start + finish);
                foreach (var boundary in boundaries)
                {
                    var wordEnd = boundary - start;
                    var wordStart = elements[start + index].Start;
                    var wordFinish = elements[boundary - 1].End;
                    var found = lexicon.TryGet(text.AsSpan(wordStart, wordFinish - wordStart), out _, out var kind);
                    Add(index, wordEnd, kind, found ? SonnetLexicalSource.Dictionary : SonnetLexicalSource.UnknownWord);
                    index = wordEnd;
                }
            }
            else
            {
                while (index < finish)
                {
                    Add(index, index + 1, kinds[index], known[index] ? SonnetLexicalSource.Dictionary : SonnetLexicalSource.UnknownWord);
                    index++;
                }
            }
        }

        void Add(int from, int to, SonnetLexicalKind kind, SonnetLexicalSource source) =>
            output.Add(SonnetTokenizer.Token(text, elements, start + from, start + to, SonnetLanguage.Chinese, kind, source));
    }
}

internal sealed class SonnetChineseHmm
{
    private const double Minimum = -3.14e100;
    private readonly byte[] _data = SonnetLanguageData.Read("ChineseHmm.bin", "SNH1");
    private readonly int[] _offsets = new int[4];
    private readonly int[] _counts = new int[4];
    // BMES. Keep tie-breaking by state name identical to upstream.
    private static readonly int[][] Previous = [[2, 3], [1, 0], [0, 1], [3, 2]];
    public static SonnetChineseHmm Instance => Holder.Value;
    private static class Holder { internal static readonly SonnetChineseHmm Value = new(); }

    private SonnetChineseHmm()
    {
        var offset = 4 + 20 * 8;
        for (var state = 0; state < 4; state++)
        {
            _counts[state] = SonnetLanguageData.Int(_data, offset);
            _offsets[state] = offset + 4;
            offset += 4 + _counts[state] * 12;
        }
    }

    public List<int> Split(SonnetTextElement[] elements, int start, int end)
    {
        var output = new List<int>();
        // No emission for a supplementary/combined glyph: preserve it individually
        // rather than allowing impossible model paths to glue arbitrary text to it.
        for (var index = start; index < end;)
        {
            if (!HasEmission(elements[index].Text)) { output.Add(++index); continue; }
            var finish = index + 1;
            while (finish < end && HasEmission(elements[finish].Text)) finish++;
            Decode(index, finish);
            index = finish;
        }
        return output;

        void Decode(int from, int to)
        {
            var count = to - from;
            var back = new byte[count * 4];
            Span<double> current = stackalloc double[4];
            Span<double> next = stackalloc double[4];
            var scalar = Rune.GetRuneAt(elements[from].Text, 0).Value;
            for (var state = 0; state < 4; state++)
                current[state] = SonnetLanguageData.Double(_data, 4 + state * 8) + Emission(state, scalar);
            for (var i = 1; i < count; i++)
            {
                scalar = Rune.GetRuneAt(elements[from + i].Text, 0).Value;
                for (var state = 0; state < 4; state++)
                {
                    var best = double.NegativeInfinity;
                    var winner = 0;
                    foreach (var previous in Previous[state])
                    {
                        var score = current[previous] + SonnetLanguageData.Double(_data, 36 + (previous * 4 + state) * 8);
                        if (score > best || score == best && "BMES"[previous] > "BMES"[winner])
                        { best = score; winner = previous; }
                    }
                    next[state] = best + Emission(state, scalar);
                    back[i * 4 + state] = (byte)winner;
                }
                next.CopyTo(current);
            }
            var last = current[2] > current[3] ? 2 : 3;
            var path = new byte[count];
            for (var i = count - 1; i >= 0; i--)
            { path[i] = (byte)last; last = back[i * 4 + last]; }
            for (var i = 0; i < count; i++)
                if (path[i] is 2 or 3) output.Add(from + i + 1);
            if (output.Count == 0 || output[^1] != to) output.Add(to);
        }
    }

    private bool HasEmission(string text)
    {
        if (text.EnumerateRunes().Count() != 1) return false;
        var scalar = Rune.GetRuneAt(text, 0).Value;
        for (var state = 0; state < 4; state++)
            if (Emission(state, scalar) > Minimum) return true;
        return false;
    }

    private double Emission(int state, int scalar)
    {
        var low = 0;
        var high = _counts[state] - 1;
        while (low <= high)
        {
            var mid = low + (high - low) / 2;
            var offset = _offsets[state] + mid * 12;
            var value = SonnetLanguageData.Int(_data, offset);
            if (value == scalar) return SonnetLanguageData.Double(_data, offset + 4);
            if (value < scalar) low = mid + 1;
            else high = mid - 1;
        }
        return Minimum;
    }
}
