using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
///     搜索专辑结果。
/// </summary>
public record SearchAlbumResponse : KgBaseModel
{
    /// <summary>
    ///     匹配到的总记录数。
    /// </summary>
    [property: JsonPropertyName("total")] public int Total { get; set; }

    /// <summary>
    ///     当前页专辑列表。
    /// </summary>
    [property: JsonPropertyName("lists")] public List<SearchAlbumItem> Albums { get; set; } = new();
}

/// <summary>
///     搜索结果中的专辑信息。
/// </summary>
public record SearchAlbumItem : KgBaseModel
{
    /// <summary>
    ///     专辑 ID。
    /// </summary>
    [property: JsonPropertyName("albumid")]
    public long AlbumId { get; set; }

    /// <summary>
    ///     专辑名称。
    /// </summary>
    [property: JsonPropertyName("albumname")]
    public string Name { get; set; } = "";

    /// <summary>
    ///     歌手名称。
    /// </summary>
    [property: JsonPropertyName("singer")] public string SingerName { get; set; } = "";

    /// <summary>
    ///     收录歌曲数。
    /// </summary>
    [property: JsonPropertyName("songcount")]
    public int SongCount { get; set; }

    /// <summary>
    ///     发布时间。
    /// </summary>
    [property: JsonPropertyName("publish_time")]
    public string PublishTime { get; set; } = "";

    /// <summary>
    ///     备注信息。
    /// </summary>
    [property: JsonPropertyName("ostremark")]
    public string Remark { get; set; } = "";

    /// <summary>
    ///     封面图地址。
    /// </summary>
    [property: JsonPropertyName("img")] public string? Cover { get; set; }
}
