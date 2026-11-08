using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
///     歌单详情。
/// </summary>
public record PlaylistInfo : KgBaseModel
{
    /// <summary>
    ///     用户歌单列表 ID。
    /// </summary>
    [property: JsonPropertyName("listid")] public long Id { get; set; }

    /// <summary>
    ///     歌单 ID。
    /// </summary>
    [property: JsonPropertyName("global_collection_id")]
    public string GlobalId { get; set; } = "";

    /// <summary>
    ///     歌单名称。
    /// </summary>
    [property: JsonPropertyName("name")] public string Name { get; set; } = "";

    /// <summary>
    ///     封面图片地址。
    /// </summary>
    [property: JsonPropertyName("pic")] public string PicUrl { get; set; } = "";

    /// <summary>
    ///     歌单简介。
    /// </summary>
    [property: JsonPropertyName("intro")] public string Intro { get; set; } = "";

    /// <summary>
    ///     歌曲总数。
    /// </summary>
    [property: JsonPropertyName("count")] public int SongCount { get; set; }

    /// <summary>
    ///     创建者昵称。
    /// </summary>
    [property: JsonPropertyName("list_create_username")]
    public string CreatorName { get; set; } = "";

    /// <summary>
    ///     创建者用户 ID。
    /// </summary>
    [property: JsonPropertyName("list_create_userid")]
    public long CreatorId { get; set; }

    /// <summary>
    ///     播放量。
    /// </summary>
    [property: JsonPropertyName("heat")] public int Heat { get; set; }

    /// <summary>
    ///     创建时间戳。
    /// </summary>
    [property: JsonPropertyName("create_time")]
    public long CreateTime { get; set; }
}
