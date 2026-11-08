using System.Text;

namespace AvaloniaSilkEffects.Sonnet;

// TinySegmenter by Taku Kudo; Python port by Masato Hagiwara / Tatsuro Yasukawa.
// BSD-3-Clause notices and pinned weights are shipped in LanguageData.
internal sealed class SonnetJapaneseTokenizer
{
    private readonly Dictionary<string, Dictionary<string, int>> _tables = new(StringComparer.Ordinal);
    private readonly int _bias;
    public static SonnetJapaneseTokenizer Instance => Holder.Value;
    private static class Holder { internal static readonly SonnetJapaneseTokenizer Value = new(); }

    private SonnetJapaneseTokenizer()
    {
        var data = SonnetLanguageData.Read("Japanese.bin", "SNJ1");
        _bias = SonnetLanguageData.Int(data, 4);
        var count = SonnetLanguageData.Int(data, 8);
        var cursor = 12;
        for (var index = 0; index < count; index++)
        {
            var name = ReadString();
            var entries = SonnetLanguageData.Int(data, cursor);
            cursor += 4;
            var table = new Dictionary<string, int>(entries, StringComparer.Ordinal);
            for (var entry = 0; entry < entries; entry++)
            {
                var key = ReadString();
                table.Add(key, SonnetLanguageData.Int(data, cursor));
                cursor += 4;
            }
            _tables.Add(name, table);
        }

        string ReadString()
        {
            var length = SonnetLanguageData.VarInt(data, ref cursor);
            var text = Encoding.UTF8.GetString(data, cursor, length);
            cursor += length;
            return text;
        }
    }

    public bool[] Boundaries(SonnetTextElement[] elements)
    {
        var boundaries = new bool[elements.Length + 1];
        boundaries[0] = boundaries[^1] = true;
        var words = new string[elements.Length + 6];
        var types = new string[words.Length];
        words[0] = "B3"; words[1] = "B2"; words[2] = "B1";
        words[^3] = "E1"; words[^2] = "E2"; words[^1] = "E3";
        Array.Fill(types, "O");
        for (var index = 0; index < elements.Length; index++)
        {
            words[index + 3] = elements[index].Text;
            types[index + 3] = CharacterType(SonnetTokenizer.FirstRune(elements[index].Text));
        }
        var p1 = "U"; var p2 = "U"; var p3 = "U";
        for (var index = 4; index < words.Length - 3; index++)
        {
            var w1 = words[index - 3]; var w2 = words[index - 2]; var w3 = words[index - 1];
            var w4 = words[index]; var w5 = words[index + 1]; var w6 = words[index + 2];
            var c1 = types[index - 3]; var c2 = types[index - 2]; var c3 = types[index - 1];
            var c4 = types[index]; var c5 = types[index + 1]; var c6 = types[index + 2];
            var score = _bias
                + Weight("UP1", p1) + Weight("UP2", p2) + Weight("UP3", p3)
                + Weight("BP1", p1 + p2) + Weight("BP2", p2 + p3)
                + Weight("UW1", w1) + Weight("UW2", w2) + Weight("UW3", w3)
                + Weight("UW4", w4) + Weight("UW5", w5) + Weight("UW6", w6)
                + Weight("BW1", w2 + w3) + Weight("BW2", w3 + w4) + Weight("BW3", w4 + w5)
                + Weight("TW1", w1 + w2 + w3) + Weight("TW2", w2 + w3 + w4)
                + Weight("TW3", w3 + w4 + w5) + Weight("TW4", w4 + w5 + w6)
                + Weight("UC1", c1) + Weight("UC2", c2) + Weight("UC3", c3)
                + Weight("UC4", c4) + Weight("UC5", c5) + Weight("UC6", c6)
                + Weight("BC1", c2 + c3) + Weight("BC2", c3 + c4) + Weight("BC3", c4 + c5)
                + Weight("TC1", c1 + c2 + c3) + Weight("TC2", c2 + c3 + c4)
                + Weight("TC3", c3 + c4 + c5) + Weight("TC4", c4 + c5 + c6)
                + Weight("UQ1", p1 + c1) + Weight("UQ2", p2 + c2) + Weight("UQ3", p3 + c3)
                + Weight("BQ1", p2 + c2 + c3) + Weight("BQ2", p2 + c3 + c4)
                + Weight("BQ3", p3 + c2 + c3) + Weight("BQ4", p3 + c3 + c4)
                + Weight("TQ1", p2 + c1 + c2 + c3) + Weight("TQ2", p2 + c2 + c3 + c4)
                + Weight("TQ3", p3 + c1 + c2 + c3) + Weight("TQ4", p3 + c2 + c3 + c4);
            var boundary = score > 0;
            boundaries[index - 3] = boundary;
            p1 = p2; p2 = p3; p3 = boundary ? "B" : "O";
        }
        return boundaries;
    }

    private int Weight(string table, string key) => _tables[table].GetValueOrDefault(key);

    private static string CharacterType(Rune rune)
    {
        var value = rune.Value;
        if ("一二三四五六七八九十百千万億兆".Contains(rune.ToString(), StringComparison.Ordinal)) return "M";
        if (value is 0x3005 or 0x3006 or 0x30F5 or 0x30F6 || SonnetTokenizer.IsHan(rune)) return "H";
        if (value is >= 0x3041 and <= 0x3093) return "I";
        if (value is >= 0x30A1 and <= 0x30F4 or >= 0xFF71 and <= 0xFF9E or 0x30FC or 0xFF70) return "K";
        if (value is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= 0xFF41 and <= 0xFF5A or >= 0xFF21 and <= 0xFF3A) return "A";
        if (value is >= '0' and <= '9' or >= 0xFF10 and <= 0xFF19) return "N";
        return "O";
    }
}
