using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
///     私人 FM 或猜你喜欢响应数据。
/// </summary>
public record PersonalFmResponse : KgBaseModel
{
    /// <summary>
    ///     推荐的歌曲列表。
    /// </summary>
    [JsonPropertyName("song_list")]
    public List<PersonalFmSong> Songs { get; set; } = new();

    /// <summary>
    ///     当前推荐模式
    /// </summary>
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "";
}

/// <summary>
///     私人 FM 单曲信息。
/// </summary>
public record PersonalFmSong : KgBaseModel
{
    /// <summary>
    ///     歌曲名称。
    /// </summary>
    [JsonPropertyName("songname")]
    public string Name { get; set; } = "";

    /// <summary>
    ///     歌手名称字符串。
    /// </summary>
    [JsonPropertyName("author_name")]
    public string SingerName { get; set; } = "";

    /// <summary>
    ///     歌曲 Hash。
    /// </summary>
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = "";

    /// <summary>
    ///     时长，单位为秒。
    /// </summary>
    [JsonPropertyName("time_length")]
    public int DurationSeconds { get; set; }

    /// <summary>
    ///     专辑 ID。
    /// </summary>
    [JsonPropertyName("album_id")]
    public string AlbumId { get; set; } = "";

    /// <summary>
    ///     音频 ID。
    /// </summary>
    [JsonPropertyName("songid")]
    public long AudioId { get; set; }

    /// <summary>
    ///     混合歌曲 ID。
    /// </summary>
    [JsonPropertyName("mixsongid")]
    public string MixSongId { get; set; } = "";

    /// <summary>
    ///     权限标记。
    /// </summary>
    [JsonPropertyName("privilege")]
    public int Privilege { get; set; }

    /// <summary>
    ///     歌手列表。
    /// </summary>
    [JsonPropertyName("singerinfo")]
    public List<SingerLite> Singers { get; set; } = new();

    /// <summary>
    ///     扩展参数，包含封面等字段。
    /// </summary>
    [JsonPropertyName("trans_param")]
    public TransParam? TransParam { get; set; }
}
