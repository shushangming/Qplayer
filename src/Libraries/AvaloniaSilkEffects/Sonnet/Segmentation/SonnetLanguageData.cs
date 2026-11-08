using System.Buffers.Binary;
using System.Text;

namespace AvaloniaSilkEffects.Sonnet;

// The lexicon remains in one prefix-compressed byte array. Only a block index is
// materialized; neither all words nor their prefixes become managed strings.
internal sealed class SonnetChineseLexicon
{
    private readonly byte[] _data = SonnetLanguageData.Read("Chinese.bin", "SNL1");
    private readonly int _count;
    private readonly int _blocks;
    private readonly int _maxBytes;
    public int MaxLength { get; }
    public int MaxUtf8Length => _maxBytes;
    public double UnknownScore { get; }
    public static SonnetChineseLexicon Instance => Holder.Value;
    private static class Holder { internal static readonly SonnetChineseLexicon Value = new(); }

    private SonnetChineseLexicon()
    {
        _count = SonnetLanguageData.Int(_data, 4);
        MaxLength = SonnetLanguageData.Int(_data, 8);
        _maxBytes = SonnetLanguageData.Int(_data, 12);
        _blocks = SonnetLanguageData.Int(_data, 16);
        UnknownScore = -SonnetLanguageData.Double(_data, 20);
    }

    public PrefixCursor Scan(Span<byte> prefix, Span<byte> word) => new(this, prefix, word);

    // A sorted dictionary's matching prefix interval acts as a trie node. The
    // cursor refines that interval instead of looking up every substring afresh.
    // Buffers belong to the caller and are reused for every sentence position.
    internal ref struct PrefixCursor
    {
        private readonly SonnetChineseLexicon _lexicon;
        private readonly Span<byte> _prefix;
        private readonly Span<byte> _word;
        private int _length;
        private int _lower;
        private int _upper;

        internal PrefixCursor(SonnetChineseLexicon lexicon, Span<byte> prefix, Span<byte> word)
        {
            _lexicon = lexicon;
            _prefix = prefix;
            _word = word;
            _length = 0;
            _lower = 0;
            _upper = lexicon._count;
        }

        // False means no dictionary word can extend this prefix. A live prefix
        // need not itself be a complete word (isWord reports that separately).
        public bool Advance(ReadOnlySpan<char> element, out bool isWord, out double score, out SonnetLexicalKind kind)
        {
            isWord = false;
            score = _lexicon.UnknownScore;
            kind = SonnetLexicalKind.Other;
            if (_lower == _upper) return false;
            var bytes = Encoding.UTF8.GetByteCount(element);
            if (bytes > _prefix.Length - _length)
            {
                _upper = _lower;
                return false;
            }
            _length += Encoding.UTF8.GetBytes(element, _prefix[_length..]);
            var key = _prefix[.._length];
            _lower = _lexicon.PrefixBound(key, _lower, _upper, false, _word);
            _upper = _lexicon.PrefixBound(key, _lower, _upper, true, _word);
            if (_lower == _upper) return false;
            var length = _lexicon.ReadEntry(_lower, _word, out var entryScore, out var entryKind);
            // A terminal sorts before all of its extensions, so only the first
            // word in the matching interval can equal the prefix.
            isWord = length == _length;
            if (isWord) { score = entryScore; kind = entryKind; }
            return true;
        }
    }

    private int PrefixBound(ReadOnlySpan<byte> key, int lower, int upper, bool exclusive, Span<byte> word)
    {
        if (lower == upper) return lower;
        var firstBlock = lower / 32;
        var left = firstBlock;
        var right = (upper - 1) / 32 + 1;
        while (left < right)
        {
            var mid = left + (right - left) / 2;
            var cursor = SonnetLanguageData.Int(_data, 28 + mid * 4);
            SonnetLanguageData.VarInt(_data, ref cursor);
            var length = SonnetLanguageData.VarInt(_data, ref cursor);
            var compare = ComparePrefix(_data.AsSpan(cursor, length), key);
            if (compare < 0 || exclusive && compare == 0) left = mid + 1;
            else right = mid;
        }
        // Block first words bracket the bound; only the preceding block needs
        // decompression. It can straddle either end of the previous interval.
        var block = Math.Max(firstBlock, left - 1);
        var position = SonnetLanguageData.Int(_data, 28 + block * 4);
        var end = Math.Min((block + 1) * 32, upper);
        for (var index = block * 32; index < end; index++)
        {
            var length = DecodeEntry(ref position, word, out _, out _);
            if (index < lower) continue;
            var compare = ComparePrefix(word[..length], key);
            if (compare > 0 || !exclusive && compare == 0) return index;
        }
        return end;
    }

    private static int ComparePrefix(ReadOnlySpan<byte> word, ReadOnlySpan<byte> prefix) =>
        word[..Math.Min(word.Length, prefix.Length)].SequenceCompareTo(prefix);

    private int ReadEntry(int index, Span<byte> word, out double score, out SonnetLexicalKind kind)
    {
        var first = index / 32 * 32;
        var position = SonnetLanguageData.Int(_data, 28 + index / 32 * 4);
        var length = 0;
        score = UnknownScore;
        kind = SonnetLexicalKind.Other;
        for (var current = first; current <= index; current++)
            length = DecodeEntry(ref position, word, out score, out kind);
        return length;
    }

    private int DecodeEntry(ref int position, Span<byte> word, out double score, out SonnetLexicalKind kind)
    {
        var prefix = SonnetLanguageData.VarInt(_data, ref position);
        var suffix = SonnetLanguageData.VarInt(_data, ref position);
        _data.AsSpan(position, suffix).CopyTo(word[prefix..]);
        position += suffix;
        score = BinaryPrimitives.ReadSingleLittleEndian(_data.AsSpan(position, 4));
        kind = (SonnetLexicalKind)_data[position + 4];
        position += 5;
        return prefix + suffix;
    }

    public bool TryGet(ReadOnlySpan<char> text, out double score, out SonnetLexicalKind kind)
    {
        score = UnknownScore;
        kind = SonnetLexicalKind.Other;
        if (text.IsEmpty || text.Length > MaxLength) return false;
        Span<byte> key = stackalloc byte[_maxBytes];
        var byteCount = Encoding.UTF8.GetByteCount(text);
        if (byteCount > key.Length) return false;
        key = key[..Encoding.UTF8.GetBytes(text, key)];
        var low = 0;
        var high = _blocks - 1;
        var block = -1;
        while (low <= high)
        {
            var mid = low + (high - low) / 2;
            var cursor = SonnetLanguageData.Int(_data, 28 + mid * 4);
            SonnetLanguageData.VarInt(_data, ref cursor); // first entry has no prefix
            var length = SonnetLanguageData.VarInt(_data, ref cursor);
            var compare = _data.AsSpan(cursor, length).SequenceCompareTo(key);
            if (compare <= 0) { block = mid; low = mid + 1; }
            else high = mid - 1;
        }
        if (block < 0) return false;
        Span<byte> word = stackalloc byte[_maxBytes];
        var position = SonnetLanguageData.Int(_data, 28 + block * 4);
        for (var index = 0; index < Math.Min(32, _count - block * 32); index++)
        {
            var prefix = SonnetLanguageData.VarInt(_data, ref position);
            var suffix = SonnetLanguageData.VarInt(_data, ref position);
            _data.AsSpan(position, suffix).CopyTo(word[prefix..]);
            position += suffix;
            var compare = word[..(prefix + suffix)].SequenceCompareTo(key);
            if (compare == 0)
            {
                score = BinaryPrimitives.ReadSingleLittleEndian(_data.AsSpan(position, 4));
                kind = (SonnetLexicalKind)_data[position + 4];
                return true;
            }
            if (compare > 0) return false;
            position += 5;
        }
        return false;
    }
}

internal static class SonnetLanguageData
{
    public static byte[] Read(string name, string signature)
    {
        using var stream = typeof(SonnetLanguageData).Assembly.GetManifestResourceStream("Sonnet.LanguageData." + name)
            ?? throw new InvalidOperationException($"Missing Sonnet language resource: {name}");
        var data = new byte[checked((int)stream.Length)];
        stream.ReadExactly(data);
        if (!data.AsSpan(0, 4).SequenceEqual(Encoding.ASCII.GetBytes(signature)))
            throw new InvalidDataException($"Invalid Sonnet language resource: {name}");
        return data;
    }

    public static int Int(byte[] data, int offset) => BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(offset, 4));
    public static double Double(byte[] data, int offset) => BinaryPrimitives.ReadDoubleLittleEndian(data.AsSpan(offset, 8));
    public static int VarInt(byte[] data, ref int cursor)
    {
        var value = 0;
        for (var shift = 0; shift < 35; shift += 7)
        {
            var next = data[cursor++];
            value |= (next & 127) << shift;
            if (next < 128) return value;
        }
        throw new InvalidDataException("Invalid Sonnet language data integer.");
    }
}
