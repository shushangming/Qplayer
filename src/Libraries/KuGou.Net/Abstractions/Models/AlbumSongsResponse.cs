using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
///     专辑歌曲响应外层 
/// </summary>
public record AlbumSongResponse : KgBaseModel
{
    /// <summary>
    ///     总歌曲数。
    /// </summary>
    [property: JsonPropertyName("total")] public int Total { get; set; }

    /// <summary>
    ///     当前页歌曲列表。
    /// </summary>
    [property: JsonPropertyName("songs")] public List<AlbumSongItem> Songs { get; set; } = new();
}

/// <summary>
///     专辑中的单首歌曲
/// </summary>
public record AlbumSongItem : KgBaseModel
{
    /// <summary>
    ///     基础信息。
    /// </summary>
    [property: JsonPropertyName("base")] public AlbumSongBase BaseInfo { get; set; } = new();

    /// <summary>
    ///     音频信息。
    /// </summary>
    [property: JsonPropertyName("audio_info")]
    public AlbumSongAudioInfo AudioInfo { get; set; } = new();

    /// <summary>
    ///     专辑信息。
    /// </summary>
    [property: JsonPropertyName("album_info")]
    public AlbumSongAlbumInfo AlbumInfo { get; set; } = new();

    /// <summary>
    ///     作者列表。
    /// </summary>
    [property: JsonPropertyName("authors")]
    public List<AlbumSongAuthor> Authors { get; set; } = new();

    /// <summary>
    ///     歌曲名称。
    /// </summary>
    [JsonIgnore] public string Name => BaseInfo.AudioName;

    /// <summary>
    ///     歌手名称。
    /// </summary>
    [JsonIgnore] public string Singer => BaseInfo.AuthorName;

    /// <summary>
    ///     歌曲 Hash。
    /// </summary>
    [JsonIgnore] public string Hash => AudioInfo.Hash;

    /// <summary>
    ///     专辑 ID。
    /// </summary>
    [JsonIgnore] public string AlbumId => BaseInfo.AlbumId.ToString();

    /// <summary>
    ///     时长，单位为毫秒。
    /// </summary>
    [JsonIgnore] public int DurationMs => AudioInfo.Duration;

    /// <summary>
    ///     封面图地址。
    /// </summary>
    [JsonIgnore] public string Cover => AlbumInfo.Cover.Replace("{size}", "400");

    /// <summary>
    ///     转换为兼容 UI 的 SingerLite 列表
    /// </summary>
    [JsonIgnore]
    public List<SingerLite> Singers => Authors.Select(a => new SingerLite
    {
        Id = a.AuthorId,
        Name = a.AuthorName
    }).ToList();
}

/// <summary>
///     专辑歌曲基础信息。
/// </summary>
public record AlbumSongBase
{
    /// <summary>
    ///     歌曲名称。
    /// </summary>
    [property: JsonPropertyName("audio_name")]
    public string AudioName { get; set; } = "";

    /// <summary>
    ///     歌手名称。
    /// </summary>
    [property: JsonPropertyName("author_name")]
    public string AuthorName { get; set; } = "";

    /// <summary>
    ///     专辑 ID。
    /// </summary>
    [property: JsonPropertyName("album_id")]
    public long AlbumId { get; set; }
}

/// <summary>
///     专辑歌曲音频信息。
/// </summary>
public record AlbumSongAudioInfo
{
    /// <summary>
    ///     歌曲 Hash。
    /// </summary>
    [property: JsonPropertyName("hash")] public string Hash { get; set; } = "";

    /// <summary>
    ///     时长，单位为毫秒。
    /// </summary>
    [property: JsonPropertyName("duration")]
    public int Duration { get; set; }
}

/// <summary>
///     专辑信息。
/// </summary>
public record AlbumSongAlbumInfo
{
    /// <summary>
    ///     专辑名称。
    /// </summary>
    [property: JsonPropertyName("album_name")]
    public string AlbumName { get; set; } = "";

    /// <summary>
    ///     封面图地址。
    /// </summary>
    [property: JsonPropertyName("cover")] public string Cover { get; set; } = "";
}

/// <summary>
///     专辑歌曲作者信息。
/// </summary>
public record AlbumSongAuthor
{
    /// <summary>
    ///     作者 ID。
    /// </summary>
    [property: JsonPropertyName("author_id")]
    public long AuthorId { get; set; }

    /// <summary>
    ///     作者名称。
    /// </summary>
    [property: JsonPropertyName("author_name")]
    public string AuthorName { get; set; } = "";
}
