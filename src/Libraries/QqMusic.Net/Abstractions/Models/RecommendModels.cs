using System.Text.Json.Serialization;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     每日推荐结果（需登录，载荷在 response.data 下）。
/// </summary>
public record QqDailyRecommend : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqSongInfo>? List { get; set; }

    [property: JsonPropertyName("songlist")]
    public List<QqSongInfo>? SongList { get; set; }

    [property: JsonPropertyName("date")]
    public string? Date { get; set; }

    [property: JsonPropertyName("uin")]
    public long? Uin { get; set; }
}

/// <summary>
///     私人 FM 结果（需登录）。
/// </summary>
public record QqPrivateFm : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqSongInfo>? List { get; set; }

    [property: JsonPropertyName("songlist")]
    public List<QqSongInfo>? SongList { get; set; }
}

/// <summary>
///     新歌速递结果。真实结构：response.new_song.data.{lanlist, lan, songlist}
/// </summary>
public record QqNewSongs : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqSongInfo>? List { get; set; }

    [property: JsonPropertyName("songlist")]
    public List<QqSongInfo>? SongList { get; set; }

    [property: JsonPropertyName("lanlist")]
    public List<QqNewSongLan>? LanList { get; set; }

    [property: JsonPropertyName("lan")]
    public string? Lan { get; set; }

    [property: JsonPropertyName("areaId")]
    public int? AreaId { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }
}

/// <summary>
///     新歌语种分类。
/// </summary>
public record QqNewSongLan
{
    [property: JsonPropertyName("name")]
    public string? Name { get; set; }

    [property: JsonPropertyName("lan")]
    public string? Lan { get; set; }

    [property: JsonPropertyName("type")]
    public int? Type { get; set; }

    [property: JsonPropertyName("tjreport")]
    public string? TjReport { get; set; }
}

/// <summary>
///     首页推荐包结果。真实结构：response.category.data.{category:[{group_id, group_name, items}]}
/// </summary>
public record QqRecommend : QqBaseModel
{
    [property: JsonPropertyName("category")]
    public List<QqRecommendGroup>? Category { get; set; }

    [property: JsonPropertyName("modules")]
    public List<QqRecommendModule>? Modules { get; set; }
}

/// <summary>
///     推荐分组。
/// </summary>
public record QqRecommendGroup
{
    [property: JsonPropertyName("group_id")]
    public int? GroupId { get; set; }

    [property: JsonPropertyName("group_name")]
    public string? GroupName { get; set; }

    [property: JsonPropertyName("items")]
    public List<QqRecommendItem>? Items { get; set; }
}

/// <summary>
///     推荐模块。
/// </summary>
public record QqRecommendModule
{
    [property: JsonPropertyName("id")]
    public string? Id { get; set; }

    [property: JsonPropertyName("title")]
    public string? Title { get; set; }

    [property: JsonPropertyName("type")]
    public string? Type { get; set; }

    [property: JsonPropertyName("sorted_list")]
    public List<QqRecommendItem>? Items { get; set; }
}

/// <summary>
///     推荐项。
/// </summary>
public record QqRecommendItem
{
    [property: JsonPropertyName("item_id")]
    public int? ItemId { get; set; }

    [property: JsonPropertyName("item_name")]
    public string? ItemName { get; set; }

    [property: JsonPropertyName("item_desc")]
    public string? ItemDesc { get; set; }

    [property: JsonPropertyName("item_new")]
    public int? ItemNew { get; set; }

    [property: JsonPropertyName("item_hot")]
    public int? ItemHot { get; set; }

    [property: JsonPropertyName("show_type")]
    public int? ShowType { get; set; }

    [property: JsonPropertyName("is_parent")]
    public int? IsParent { get; set; }

    [property: JsonPropertyName("reimgurl")]
    public string? ReImgUrl { get; set; }

    [property: JsonPropertyName("item_share_pic")]
    public string? ItemSharePic { get; set; }

    [property: JsonPropertyName("show_detail")]
    public int? ShowDetail { get; set; }

    // 兼容旧字段
    [property: JsonPropertyName("id")]
    public string? Id { get; set; }

    [property: JsonPropertyName("type")]
    public int? Type { get; set; }

    [property: JsonPropertyName("title")]
    public string? Title { get; set; }

    [property: JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [property: JsonPropertyName("jumpUrl")]
    public string? JumpUrl { get; set; }
}

/// <summary>
///     推荐 Banner。真实结构：response.focus.data.{content:[...]}
/// </summary>
public record QqRecommendBanner : QqBaseModel
{
    [property: JsonPropertyName("content")]
    public List<QqBannerItem>? Content { get; set; }

    [property: JsonPropertyName("list")]
    public List<QqBannerItem>? List { get; set; }

    [property: JsonPropertyName("banners")]
    public List<QqBannerItem>? Banners { get; set; }
}

/// <summary>
///     Banner 项。
/// </summary>
public record QqBannerItem
{
    [property: JsonPropertyName("id")]
    public int? Id { get; set; }

    [property: JsonPropertyName("title")]
    public string? Title { get; set; }

    [property: JsonPropertyName("sub_title")]
    public string? SubTitle { get; set; }

    [property: JsonPropertyName("type")]
    public int? Type { get; set; }

    [property: JsonPropertyName("cover")]
    public string? Cover { get; set; }

    [property: JsonPropertyName("listen_num")]
    public long? ListenNum { get; set; }

    [property: JsonPropertyName("report")]
    public string? Report { get; set; }

    [property: JsonPropertyName("pic_info")]
    public QqBannerPicInfo? PicInfo { get; set; }

    [property: JsonPropertyName("jump_info")]
    public QqBannerJumpInfo? JumpInfo { get; set; }

    // 兼容旧字段
    [property: JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [property: JsonPropertyName("jumpUrl")]
    public string? JumpUrl { get; set; }
}

/// <summary>
///     Banner 图片信息。
/// </summary>
public record QqBannerPicInfo
{
    [property: JsonPropertyName("mid")]
    public string? Mid { get; set; }

    [property: JsonPropertyName("url")]
    public string? Url { get; set; }

    [property: JsonPropertyName("urlex1")]
    public string? UrlEx1 { get; set; }

    [property: JsonPropertyName("urlex2")]
    public string? UrlEx2 { get; set; }
}

/// <summary>
///     Banner 跳转信息。
/// </summary>
public record QqBannerJumpInfo
{
    [property: JsonPropertyName("id")]
    public int? Id { get; set; }

    [property: JsonPropertyName("mid")]
    public string? Mid { get; set; }

    [property: JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>
///     个性化推荐结果（需登录）。
/// </summary>
public record QqPersonalRecommend : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqSongInfo>? Songs { get; set; }

    [property: JsonPropertyName("songlist")]
    public List<QqSongInfo>? SongList { get; set; }

    [property: JsonPropertyName("playlist")]
    public List<QqPlaylistLite>? Playlists { get; set; }

    [property: JsonPropertyName("type")]
    public string? Type { get; set; }
}

/// <summary>
///     相似歌曲结果（需登录，真实结构：response.similarSong.data 或错误码）。
/// </summary>
public record QqSimilarSongs : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqSongInfo>? List { get; set; }

    [property: JsonPropertyName("songlist")]
    public List<QqSongInfo>? SongList { get; set; }

    [property: JsonPropertyName("songmid")]
    public string? SongMid { get; set; }
}

/// <summary>
///     相关歌单结果。真实结构：response.gedan.data.{vec_gedan, has_more}
/// </summary>
public record QqRelatedPlaylists : QqBaseModel
{
    [property: JsonPropertyName("vec_gedan")]
    public List<QqPlaylistLite>? VecGedan { get; set; }

    [property: JsonPropertyName("has_more")]
    public int? HasMore { get; set; }

    [property: JsonPropertyName("list")]
    public List<QqPlaylistLite>? List { get; set; }

    [property: JsonPropertyName("songid")]
    public long? SongId { get; set; }
}
