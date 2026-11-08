using System.Text.Json.Serialization;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     用户信息。
/// </summary>
public record QqUserInfo : QqBaseModel
{
    [property: JsonPropertyName("uin")]
    public long? Uin { get; set; }

    [property: JsonPropertyName("encrypt_uin")]
    public string? EncryptUin { get; set; }

    [property: JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [property: JsonPropertyName("name")]
    public string? Name { get; set; }

    [property: JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [property: JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [property: JsonPropertyName("gender")]
    public int? Gender { get; set; }

    [property: JsonPropertyName("country")]
    public string? Country { get; set; }

    [property: JsonPropertyName("province")]
    public string? Province { get; set; }

    [property: JsonPropertyName("city")]
    public string? City { get; set; }

    [property: JsonPropertyName("signature")]
    public string? Signature { get; set; }

    [property: JsonPropertyName("fans")]
    public long? Fans { get; set; }

    [property: JsonPropertyName("follows")]
    public long? Follows { get; set; }

    [property: JsonPropertyName("songnum")]
    public int? SongNum { get; set; }

    [property: JsonPropertyName("listennum")]
    public long? ListenNum { get; set; }
}

/// <summary>
///     用户歌单列表结果。
///     真实结构（getUserPlaylists）：response.data.{ playlists: [...] }（复数 playlists）。
/// </summary>
public record QqUserPlaylists : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqPlaylistLite>? List { get; set; }

    [property: JsonPropertyName("playlists")]
    public List<QqPlaylistLite>? Playlists { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }

    [property: JsonPropertyName("uin")]
    public long? Uin { get; set; }
}

/// <summary>
///     用户收藏歌单结果。
/// </summary>
public record QqUserCollectedPlaylists : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqPlaylistLite>? List { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }
}

/// <summary>
///     用户关注歌手结果。
/// </summary>
public record QqUserFollowSingers : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqSingerInfo>? List { get; set; }

    [property: JsonPropertyName("singers")]
    public List<QqSingerInfo>? Singers { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }
}

/// <summary>
///     用户头像结果。
/// </summary>
public record QqUserAvatar : QqBaseModel
{
    [property: JsonPropertyName("uin")]
    public long? Uin { get; set; }

    [property: JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [property: JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [property: JsonPropertyName("sUrl")]
    public string? SmallUrl { get; set; }

    [property: JsonPropertyName("mUrl")]
    public string? MediumUrl { get; set; }

    [property: JsonPropertyName("lUrl")]
    public string? LargeUrl { get; set; }
}

/// <summary>
///     用户喜欢的音乐结果。
/// </summary>
public record QqUserLikedSongs : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqSongInfo>? List { get; set; }

    [property: JsonPropertyName("songlist")]
    public List<QqSongInfo>? SongList { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }

    [property: JsonPropertyName("songids")]
    public string? SongIds { get; set; }
}

/// <summary>
///     VIP 信息。
/// </summary>
public record QqVipInfo : QqBaseModel
{
    [property: JsonPropertyName("vip_type")]
    public int? VipType { get; set; }

    [property: JsonPropertyName("vip_level")]
    public int? VipLevel { get; set; }

    [property: JsonPropertyName("is_vip")]
    public int? IsVip { get; set; }

    [property: JsonPropertyName("is_svip")]
    public int? IsSvip { get; set; }

    [property: JsonPropertyName("is_gray_vip")]
    public int? IsGrayVip { get; set; }

    [property: JsonPropertyName("vip_start")]
    public long? VipStart { get; set; }

    [property: JsonPropertyName("vip_end")]
    public long? VipEnd { get; set; }

    [property: JsonPropertyName("pay_time")]
    public long? PayTime { get; set; }

    [property: JsonPropertyName("overdue")]
    public int? Overdue { get; set; }

    [property: JsonPropertyName("uin")]
    public long? Uin { get; set; }
}

/// <summary>
///     Cookie 信息。
/// </summary>
public record QqCookieInfo : QqBaseModel
{
    [property: JsonPropertyName("cookie")]
    public string? Cookie { get; set; }

    [property: JsonPropertyName("uin")]
    public long? Uin { get; set; }

    [property: JsonPropertyName("nickname")]
    public string? Nickname { get; set; }
}

/// <summary>
///     QQ 官方个人信息主页（fcg_get_profile_homepage.fcg）响应。
///     用于解析"我喜欢的音乐"歌单 disstid（上游 /user/getUserLikedSongs 依赖服务端配置 cookie，客户端不可用）。
/// </summary>
public record QqProfileHomepage
{
    [property: JsonPropertyName("code")]
    public int? Code { get; set; }

    [property: JsonPropertyName("data")]
    public QqProfileHomepageData? Data { get; set; }
}

/// <summary>
///     个人信息主页 data 段。
/// </summary>
public record QqProfileHomepageData
{
    [property: JsonPropertyName("mymusic")]
    public List<QqProfileMusicEntry>? Mymusic { get; set; }
}

/// <summary>
///     "我的音乐"入口条目（mymusic 数组元素）。
///     title=我喜欢 且 type=1 的条目，其 id 即"我喜欢"歌单的 disstid。
/// </summary>
public record QqProfileMusicEntry
{
    [property: JsonPropertyName("title")]
    public string? Title { get; set; }

    [property: JsonPropertyName("id")]
    public long? Id { get; set; }

    [property: JsonPropertyName("type")]
    public int? Type { get; set; }

    [property: JsonPropertyName("num0")]
    public int? Num0 { get; set; }

    [property: JsonPropertyName("num1")]
    public int? Num1 { get; set; }

    [property: JsonPropertyName("picurl")]
    public string? PicUrl { get; set; }
}
