using System.Text.Json;
using System.Text.Json.Serialization;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     歌词内容对象（isFormat=1 时 lyric 为对象；当前版本 API 始终返回对象）。
/// </summary>
public record QqLyricContent
{
    [property: JsonPropertyName("lyric")]
    public string? Lyric { get; set; }

    [property: JsonPropertyName("tags")]
    public QqLyricTags? Tags { get; set; }

    [property: JsonPropertyName("lines")]
    public List<QqLyricLine>? Lines { get; set; }
}

/// <summary>
///     歌词标签信息。
/// </summary>
public record QqLyricTags
{
    [property: JsonPropertyName("title")]
    public string? Title { get; set; }

    [property: JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [property: JsonPropertyName("album")]
    public string? Album { get; set; }

    [property: JsonPropertyName("offset")]
    public string? Offset { get; set; }

    [property: JsonPropertyName("by")]
    public string? By { get; set; }
}

/// <summary>
///     歌词行（带时间戳）。
/// </summary>
public record QqLyricLine
{
    [property: JsonPropertyName("time")]
    public long? Time { get; set; }

    [property: JsonPropertyName("txt")]
    public string? Txt { get; set; }
}

/// <summary>
///     歌词结果。
///     注意：getLyric 接口无 data 封装，载荷直接在 response 下；
///     lyric 字段为对象（含 lyric 文本、tags、lines），兼容 isFormat=0/1。
/// </summary>
public record QqLyricResult : QqBaseModel
{
    /// <summary>
    ///     歌词内容对象（兼容字符串和对象两种形式）。
    /// </summary>
    [property: JsonPropertyName("lyric")]
    public JsonElement? LyricElement { get; set; }

    [property: JsonPropertyName("trans")]
    public string? Trans { get; set; }

    [property: JsonPropertyName("romansong")]
    public string? RomanSong { get; set; }

    [property: JsonPropertyName("songmid")]
    public string? SongMid { get; set; }

    [property: JsonPropertyName("retcode")]
    public int? RetCode { get; set; }

    [property: JsonPropertyName("ret")]
    public int? Ret { get; set; }

    [property: JsonPropertyName("subcode")]
    public int? SubCode { get; set; }

    /// <summary>
    ///     获取歌词纯文本。兼容 lyric 为字符串或对象（对象时取 .lyric 子字段）。
    /// </summary>
    [JsonIgnore]
    public string? LyricText
    {
        get
        {
            if (!LyricElement.HasValue) return null;
            var el = LyricElement.Value;
            if (el.ValueKind == JsonValueKind.String)
                return el.GetString();
            if (el.ValueKind == JsonValueKind.Object &&
                el.TryGetProperty("lyric", out var inner) &&
                inner.ValueKind == JsonValueKind.String)
                return inner.GetString();
            return null;
        }
    }

    /// <summary>
    ///     获取结构化歌词内容（当 lyric 为对象时）。
    /// </summary>
    [JsonIgnore]
    public QqLyricContent? LyricContent
    {
        get
        {
            if (!LyricElement.HasValue || LyricElement.Value.ValueKind != JsonValueKind.Object)
                return null;
            return LyricElement.Value.Deserialize<QqLyricContent>();
        }
    }
}
