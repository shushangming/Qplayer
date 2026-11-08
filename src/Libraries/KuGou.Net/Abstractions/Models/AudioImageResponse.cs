using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
///     歌曲作者写真，对应 /images/audio。
/// </summary>
public record AudioImageResponse : KgBaseModel
{
    /// <summary>
    ///     错误码。
    /// </summary>
    [property: JsonPropertyName("errcode")] public int ErrCode { get; set; }

    /// <summary>
    ///     错误信息。
    /// </summary>
    [property: JsonPropertyName("errmsg")] public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    ///     作者写真分组数据。
    /// </summary>
    [property: JsonPropertyName("data")] public List<List<AudioImageAuthor>> Data { get; set; } = new();

    /// <summary>
    ///     展平后的作者写真列表。
    /// </summary>
    [JsonIgnore] public IEnumerable<AudioImageAuthor> Authors => Data.SelectMany(group => group);
}

/// <summary>
///     作者写真信息。
/// </summary>
public record AudioImageAuthor
{
    /// <summary>
    ///     作者 ID。
    /// </summary>
    [property: JsonPropertyName("author_id")] public long AuthorId { get; set; }

    /// <summary>
    ///     作者名称。
    /// </summary>
    [property: JsonPropertyName("author_name")] public string AuthorName { get; set; } = string.Empty;

    /// <summary>
    ///     是否已发布。
    /// </summary>
    [property: JsonPropertyName("is_publish")] public int IsPublish { get; set; }

    /// <summary>
    ///     资源 Hash。
    /// </summary>
    [property: JsonPropertyName("res_hash")] public string ResourceHash { get; set; } = string.Empty;

    /// <summary>
    ///     头像地址。
    /// </summary>
    [property: JsonPropertyName("avatar")] public string Avatar { get; set; } = string.Empty;

    /// <summary>
    ///     可变尺寸头像地址。
    /// </summary>
    [property: JsonPropertyName("sizable_avatar")] public string SizableAvatar { get; set; } = string.Empty;

    /// <summary>
    ///     音频发布日期。
    /// </summary>
    [property: JsonPropertyName("audio_publish_date")] public string AudioPublishDate { get; set; } = string.Empty;

    /// <summary>
    ///     作者写真资源集合。
    /// </summary>
    [property: JsonPropertyName("imgs")] public Dictionary<string, List<AudioImageItem>> Images { get; set; } = new();
}

/// <summary>
///     单张写真资源。
/// </summary>
public record AudioImageItem
{
    /// <summary>
    ///     图片 ID。
    /// </summary>
    [property: JsonPropertyName("id")] public long Id { get; set; }

    /// <summary>
    ///     文件 Hash。
    /// </summary>
    [property: JsonPropertyName("file_hash")] public string FileHash { get; set; } = string.Empty;

    /// <summary>
    ///     可变尺寸写真地址。
    /// </summary>
    [property: JsonPropertyName("sizable_portrait")] public string SizablePortrait { get; set; } = string.Empty;

    /// <summary>
    ///     文件名。
    /// </summary>
    [property: JsonPropertyName("filename")] public string FileName { get; set; } = string.Empty;

    /// <summary>
    ///     发布时间。
    /// </summary>
    [property: JsonPropertyName("publish_time")] public string PublishTime { get; set; } = string.Empty;

    /// <summary>
    ///     来源标记。
    /// </summary>
    [property: JsonPropertyName("source")] public int Source { get; set; }
}
