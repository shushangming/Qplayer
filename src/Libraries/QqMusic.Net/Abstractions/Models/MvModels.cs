using System.Text.Json.Serialization;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     MV 简要信息。
/// </summary>
public record QqMvLite
{
    [property: JsonPropertyName("vid")]
    public string Vid { get; set; } = "";

    [property: JsonPropertyName("mv_id")]
    public string? MvId { get; set; }

    [property: JsonPropertyName("id")]
    public string? Id { get; set; }

    [property: JsonPropertyName("title")]
    public string? Title { get; set; }

    [property: JsonPropertyName("mvtitle")]
    public string? MvTitle { get; set; }

    [property: JsonPropertyName("mv_name")]
    public string? MvName { get; set; }

    [property: JsonPropertyName("mvdesc")]
    public string? MvDesc { get; set; }

    [property: JsonPropertyName("desc")]
    public string? Desc { get; set; }

    [property: JsonPropertyName("singer")]
    public List<QqSingerLite>? Singers { get; set; }

    [property: JsonPropertyName("singers")]
    public List<QqSingerLite>? Singers2 { get; set; }

    [property: JsonPropertyName("singer_name")]
    public string? SingerName { get; set; }

    [property: JsonPropertyName("singername")]
    public string? SingerName2 { get; set; }

    [property: JsonPropertyName("singer_id")]
    public long? SingerId { get; set; }

    [property: JsonPropertyName("singerid")]
    public long? SingerId2 { get; set; }

    [property: JsonPropertyName("singer_mid")]
    public string? SingerMid { get; set; }

    [property: JsonPropertyName("singermid")]
    public string? SingerMid2 { get; set; }

    [property: JsonPropertyName("cover")]
    public string? Cover { get; set; }

    [property: JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [property: JsonPropertyName("picurl")]
    public string? PicUrl { get; set; }

    [property: JsonPropertyName("play_num")]
    public long? PlayNum { get; set; }

    [property: JsonPropertyName("listennum")]
    public long? ListenNum { get; set; }

    [property: JsonPropertyName("listenCount")]
    public string? ListenCount { get; set; }

    [property: JsonPropertyName("mvscore")]
    public int? MvScore { get; set; }

    [property: JsonPropertyName("duration")]
    public int? Duration { get; set; }

    [property: JsonPropertyName("pub_date")]
    public string? PubDate { get; set; }

    [property: JsonPropertyName("publictime")]
    public string? PublicTime { get; set; }

    [property: JsonPropertyName("date")]
    public string? Date { get; set; }

    [property: JsonPropertyName("index")]
    public int? Index { get; set; }
}

/// <summary>
///     MV 播放地址结果。真实结构：response.getMVUrl.data.{vid:{mp4:[...]}}
/// </summary>
public record QqMvPlay : QqBaseModel
{
    [property: JsonPropertyName("vid")]
    public string? Vid { get; set; }

    [property: JsonPropertyName("mp4")]
    public List<QqMvPlayUrl>? Mp4Urls { get; set; }
}

/// <summary>
///     MV 播放地址项。
/// </summary>
public record QqMvPlayUrl
{
    [property: JsonPropertyName("fileSize")]
    public long? FileSize { get; set; }

    [property: JsonPropertyName("definition")]
    public string? Definition { get; set; }

    [property: JsonPropertyName("url")]
    public List<string>? Url { get; set; }

    [property: JsonPropertyName("freeflow_url")]
    public List<string>? FreeflowUrl { get; set; }

    [property: JsonPropertyName("comm_url")]
    public List<string>? CommUrl { get; set; }

    [property: JsonPropertyName("cn")]
    public string? Cn { get; set; }

    [property: JsonPropertyName("vkey")]
    public string? Vkey { get; set; }

    [property: JsonPropertyName("expire")]
    public int? Expire { get; set; }

    [property: JsonPropertyName("code")]
    public int? Code { get; set; }

    [property: JsonPropertyName("filetype")]
    public int? FileType { get; set; }

    [property: JsonPropertyName("m3u8")]
    public string? M3u8 { get; set; }

    [property: JsonPropertyName("newFileType")]
    public int? NewFileType { get; set; }

    [property: JsonPropertyName("format")]
    public int? Format { get; set; }

    [property: JsonPropertyName("videoType")]
    public int? VideoType { get; set; }
}

/// <summary>
///     MV 列表结果（getMv，真实结构：response.{mv_tag:{data}, mv_list:{data:{list,total}}}）。
/// </summary>
public record QqMvList : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqMvLite>? List { get; set; }

    [property: JsonPropertyName("mvlist")]
    public List<QqMvLite>? MvList { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }

    [property: JsonPropertyName("totalnum")]
    public int? TotalNum { get; set; }
}

/// <summary>
///     MV 分类（载荷在 response.mv_tag.data 下）。
/// </summary>
public record QqMvCategory : QqBaseModel
{
    [property: JsonPropertyName("area")]
    public List<QqMvCategoryItem>? Areas { get; set; }

    [property: JsonPropertyName("version")]
    public List<QqMvCategoryItem>? Versions { get; set; }

    [property: JsonPropertyName("type")]
    public List<QqMvCategoryItem>? Types { get; set; }
}

/// <summary>
///     MV 分类项。
/// </summary>
public record QqMvCategoryItem
{
    [property: JsonPropertyName("id")]
    public int? Id { get; set; }

    [property: JsonPropertyName("name")]
    public string? Name { get; set; }

    [property: JsonPropertyName("value")]
    public string? Value { get; set; }
}

/// <summary>
///     按标签获取 MV 结果（载荷在 response.data 下）。
/// </summary>
public record QqMvByTag : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqMvLite>? List { get; set; }

    [property: JsonPropertyName("mvlist")]
    public List<QqMvLite>? MvList { get; set; }

    [property: JsonPropertyName("lan")]
    public string? Lan { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }
}
