using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
///     从歌单删除歌曲后的结果。
/// </summary>
public record RemoveSongResponse : KgBaseModel
{
    /// <summary>
    ///     删除后歌单内的歌曲总数。
    /// </summary>
    [property: JsonPropertyName("count")] public int Count { get; set; }

    /// <summary>
    ///     目标歌单 ID。
    /// </summary>
    [property: JsonPropertyName("listid")] public long ListId { get; set; }

    /// <summary>
    ///     最后更新时间戳。
    /// </summary>
    [property: JsonPropertyName("last_time")]
    public long LastTime { get; set; }
}
