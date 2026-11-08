using System.Text.Json;
using System.Text.Json.Serialization;
using QqMusic.Net.util;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     歌单分类。载荷在 response.data.categories 下。
///     真实结构：categories:[{categoryGroupName, items:[{categoryId, categoryName, allsorts, usable}], usable}]
/// </summary>
public record QqPlaylistCategory : QqBaseModel
{
    [property: JsonPropertyName("categories")]
    public List<QqPlaylistCategoryGroup>? Categories { get; set; }
}

/// <summary>
///     歌单分类组。
/// </summary>
public record QqPlaylistCategoryGroup
{
    [property: JsonPropertyName("categoryGroupName")]
    public string? CategoryGroupName { get; set; }

    [property: JsonPropertyName("items")]
    public List<QqPlaylistCategoryItem>? Items { get; set; }

    [property: JsonPropertyName("usable")]
    public int? Usable { get; set; }
}

/// <summary>
///     歌单分类项。
/// </summary>
public record QqPlaylistCategoryItem
{
    [property: JsonPropertyName("categoryId")]
    public int? CategoryId { get; set; }

    [property: JsonPropertyName("categoryName")]
    public string? CategoryName { get; set; }

    [property: JsonPropertyName("allsorts")]
    public List<QqPlaylistSort>? AllSorts { get; set; }

    [property: JsonPropertyName("usable")]
    public int? Usable { get; set; }
}

/// <summary>
///     歌单排序方式。
/// </summary>
public record QqPlaylistSort
{
    [property: JsonPropertyName("sortId")]
    public int? SortId { get; set; }

    [property: JsonPropertyName("sortName")]
    public string? SortName { get; set; }
}

/// <summary>
///     歌单简要信息。
///     注意：不同上游接口字段风格不同——公开歌单列表(getSongLists)用 dissname/imgurl，
///     用户歌单列表(getUserPlaylists)用 title/picurl，本类型同时兼容两者。
/// </summary>
public record QqPlaylistLite
{
    [property: JsonPropertyName("dissid")]
    [property: JsonConverter(typeof(NumberToStringConverter))]
    public string DissId { get; set; } = "";

    [property: JsonPropertyName("createtime")]
    public string? CreateTime { get; set; }

    [property: JsonPropertyName("commit_time")]
    public string? CommitTime { get; set; }

    [property: JsonPropertyName("dissname")]
    public string? DissName { get; set; }

    [property: JsonPropertyName("title")]
    public string? Title { get; set; }

    [property: JsonPropertyName("imgurl")]
    public string? ImgUrl { get; set; }

    [property: JsonPropertyName("picurl")]
    public string? PicUrl { get; set; }

    [property: JsonPropertyName("subtitle")]
    public string? Subtitle { get; set; }

    [property: JsonPropertyName("introduction")]
    public string? Introduction { get; set; }

    [property: JsonPropertyName("listennum")]
    public long? ListenNum { get; set; }

    [property: JsonPropertyName("score")]
    public int? Score { get; set; }

    [property: JsonPropertyName("version")]
    public int? Version { get; set; }

    [property: JsonPropertyName("creator")]
    public QqPlaylistCreator? Creator { get; set; }

    /// <summary>
    ///     显示名称（优先 title，其次 dissname）。
    /// </summary>
    [JsonIgnore]
    public string DisplayName => string.IsNullOrWhiteSpace(Title) ? DissName ?? "" : Title;

    /// <summary>
    ///     封面地址（优先 picurl，其次 imgurl）。
    /// </summary>
    [JsonIgnore]
    public string DisplayCover => string.IsNullOrWhiteSpace(PicUrl) ? ImgUrl ?? "" : PicUrl;
}

/// <summary>
///     歌单创建者。
/// </summary>
public record QqPlaylistCreator
{
    [property: JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [property: JsonPropertyName("encrypt_uin")]
    public string? EncryptUin { get; set; }

    [property: JsonPropertyName("name")]
    public string? Name { get; set; }

    [property: JsonPropertyName("qq")]
    public long? QQ { get; set; }

    [property: JsonPropertyName("type")]
    public int? Type { get; set; }

    [property: JsonPropertyName("uin")]
    public long? Uin { get; set; }
}

/// <summary>
///     歌单列表结果。
/// </summary>
public record QqPlaylistList : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqPlaylistLite>? List { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }

    [property: JsonPropertyName("sum")]
    public int? Sum { get; set; }
}

/// <summary>
///     歌单详情。
/// </summary>
public record QqPlaylistDetail : QqBaseModel
{
    [property: JsonPropertyName("disstid")]
    public string? DissId { get; set; }

    [property: JsonPropertyName("dissname")]
    public string? DissName { get; set; }

    [property: JsonPropertyName("imgurl")]
    public string? ImgUrl { get; set; }

    [property: JsonPropertyName("introduction")]
    public string? Introduction { get; set; }

    [property: JsonPropertyName("listennum")]
    public long? ListenNum { get; set; }

    [property: JsonPropertyName("songlist")]
    public List<QqSongInfo>? SongList { get; set; }

    [property: JsonPropertyName("songids")]
    public string? SongIds { get; set; }

    [property: JsonPropertyName("creator")]
    public QqPlaylistCreator? Creator { get; set; }

    [property: JsonPropertyName("ctime")]
    public long? CTime { get; set; }

    [property: JsonPropertyName("version")]
    public int? Version { get; set; }
}

/// <summary>
///     批量获取歌单请求体。
/// </summary>
public record QqBatchPlaylistRequest
{
    [property: JsonPropertyName("ids")]
    public List<string> Ids { get; set; } = new();
}

/// <summary>
///     歌单标签。
/// </summary>
public record QqPlaylistTag : QqBaseModel
{
    [property: JsonPropertyName("id")]
    public string? Id { get; set; }

    [property: JsonPropertyName("name")]
    public string? Name { get; set; }

    [property: JsonPropertyName("type")]
    public int? Type { get; set; }

    [property: JsonPropertyName("hot")]
    public int? Hot { get; set; }
}

/// <summary>
///     歌单标签列表。
/// </summary>
public record QqPlaylistTagList : QqBaseModel
{
    [property: JsonPropertyName("tags")]
    public List<QqPlaylistTag>? Tags { get; set; }
}

/// <summary>
///     按标签获取歌单结果。
/// </summary>
public record QqPlaylistByTag : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqPlaylistLite>? List { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }
}

/// <summary>
///     歌单分享链接解析结果。
/// </summary>
public record QqPlaylistShareResolve : QqBaseModel
{
    [property: JsonPropertyName("disstid")]
    public string? DissId { get; set; }

    [property: JsonPropertyName("dissname")]
    public string? DissName { get; set; }

    [property: JsonPropertyName("url")]
    public string? Url { get; set; }
}
