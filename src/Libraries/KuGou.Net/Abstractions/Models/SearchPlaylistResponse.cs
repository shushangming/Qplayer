using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
///     搜索歌单结果。
/// </summary>
public record SearchPlaylistResponse : KgBaseModel
{
    /// <summary>
    ///     匹配到的总记录数。
    /// </summary>
    [property: JsonPropertyName("total")] public int Total { get; set; }

    /// <summary>
    ///     当前页歌单列表。
    /// </summary>
    [property: JsonPropertyName("lists")] public List<SearchPlaylistItem> Playlists { get; set; } = new();
}

/// <summary>
///     搜索结果中的歌单信息。
/// </summary>
public record SearchPlaylistItem : KgBaseModel
{
    /// <summary>
    ///     歌单名称。
    /// </summary>
    [property: JsonPropertyName("specialname")]
    public string Name { get; set; } = "";

    /// <summary>
    ///     歌单数字 ID。
    /// </summary>
    [property: JsonPropertyName("specialid")]
    public long ListId { get; set; }

    /// <summary>
    ///     歌单全局 ID。
    /// </summary>
    [property: JsonPropertyName("gid")] public string GlobalId { get; set; } = "";

    /// <summary>
    ///     歌曲数量。
    /// </summary>
    [property: JsonPropertyName("song_count")]
    public int SongCount { get; set; }

    /// <summary>
    ///     创建者昵称。
    /// </summary>
    [property: JsonPropertyName("nickname")]
    public string CreatorName { get; set; } = "";

    /// <summary>
    ///     播放量。
    /// </summary>
    [property: JsonPropertyName("total_play_count")]
    public long PlayCount { get; set; }

    /// <summary>
    ///     封面图地址。
    /// </summary>
    [property: JsonPropertyName("img")] public string? Cover { get; set; }

    /// <summary>
    ///     发布时间。
    /// </summary>
    [property: JsonPropertyName("publish_time")]
    public string PublishTime { get; set; } = "";
}
