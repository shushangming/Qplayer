using System.Text.Json.Serialization;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     歌手信息。
/// </summary>
public record QqSingerInfo : QqBaseModel
{
    [property: JsonPropertyName("singer_mid")]
    public string SingerMid { get; set; } = "";

    [property: JsonPropertyName("singer_id")]
    public long? SingerId { get; set; }

    [property: JsonPropertyName("singer_name")]
    public string? SingerName { get; set; }

    [property: JsonPropertyName("name")]
    public string? Name { get; set; }

    [property: JsonPropertyName("mid")]
    public string? Mid { get; set; }

    [property: JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [property: JsonPropertyName("singer_pic")]
    public string? SingerPic { get; set; }

    [property: JsonPropertyName("fans")]
    public long? Fans { get; set; }

    [property: JsonPropertyName("country")]
    public string? Country { get; set; }

    [property: JsonPropertyName("songnum")]
    public int? SongNum { get; set; }

    [property: JsonPropertyName("albumnum")]
    public int? AlbumNum { get; set; }

    [property: JsonPropertyName("mvnum")]
    public int? MvNum { get; set; }

    [property: JsonPropertyName("index")]
    public string? Index { get; set; }
}

/// <summary>
///     歌手列表结果（载荷在 response.singerList.data 下）。
/// </summary>
public record QqSingerList : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqSingerInfo>? List { get; set; }

    [property: JsonPropertyName("singerlist")]
    public List<QqSingerInfo>? SingerList { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }

    [property: JsonPropertyName("totalnum")]
    public int? TotalNum { get; set; }

    [property: JsonPropertyName("curnum")]
    public int? CurNum { get; set; }

    [property: JsonPropertyName("curpage")]
    public int? CurPage { get; set; }

    [property: JsonPropertyName("area")]
    public int? Area { get; set; }

    [property: JsonPropertyName("genre")]
    public int? Genre { get; set; }

    [property: JsonPropertyName("index")]
    public int? Index { get; set; }

    [property: JsonPropertyName("sex")]
    public int? Sex { get; set; }
}

/// <summary>
///     歌手分类（载荷在 response.singerList.data 下）。
///     真实结构：area/genre/index/sex 为当前过滤值（数字），singerlist 为歌手列表。
/// </summary>
public record QqSingerCategory : QqBaseModel
{
    [property: JsonPropertyName("area")]
    public int? Area { get; set; }

    [property: JsonPropertyName("genre")]
    public int? Genre { get; set; }

    [property: JsonPropertyName("index")]
    public int? Index { get; set; }

    [property: JsonPropertyName("sex")]
    public int? Sex { get; set; }

    [property: JsonPropertyName("singerlist")]
    public List<QqSingerInfo>? SingerList { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }
}

/// <summary>
///     歌手分类项。
/// </summary>
public record QqSingerCategoryItem
{
    [property: JsonPropertyName("id")]
    public int? Id { get; set; }

    [property: JsonPropertyName("name")]
    public string? Name { get; set; }

    [property: JsonPropertyName("value")]
    public string? Value { get; set; }
}

/// <summary>
///     歌手简介。
/// </summary>
public record QqSingerDesc : QqBaseModel
{
    [property: JsonPropertyName("singer_id")]
    public long? SingerId { get; set; }

    [property: JsonPropertyName("singer_mid")]
    public string? SingerMid { get; set; }

    [property: JsonPropertyName("singer_name")]
    public string? SingerName { get; set; }

    [property: JsonPropertyName("desc")]
    public string? Desc { get; set; }

    [property: JsonPropertyName("info")]
    public string? Info { get; set; }
}

/// <summary>
///     歌手热门歌曲结果。
/// </summary>
public record QqSingerHotSongs : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqSongInfo>? List { get; set; }

    [property: JsonPropertyName("songlist")]
    public List<QqSongInfo>? SongList { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }

    [property: JsonPropertyName("singermid")]
    public string? SingerMid { get; set; }

    [property: JsonPropertyName("singername")]
    public string? SingerName { get; set; }
}

/// <summary>
///     歌手专辑结果（载荷在 response.singer.data 下）。
/// </summary>
public record QqSingerAlbums : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqAlbumLite>? List { get; set; }

    [property: JsonPropertyName("albumList")]
    public List<QqAlbumLite>? AlbumList { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }

    [property: JsonPropertyName("singerMid")]
    public string? SingerMid { get; set; }
}

/// <summary>
///     歌手 MV 结果。
/// </summary>
public record QqSingerMvs : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqMvLite>? List { get; set; }

    [property: JsonPropertyName("mvlist")]
    public List<QqMvLite>? MvList { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }
}

/// <summary>
///     相似歌手结果。
/// </summary>
public record QqSimilarSingers : QqBaseModel
{
    [property: JsonPropertyName("list")]
    public List<QqSingerInfo>? List { get; set; }

    [property: JsonPropertyName("singers")]
    public List<QqSingerInfo>? Singers { get; set; }
}

/// <summary>
///     歌手星数（人气）结果（载荷在 response 下，无 data 封装）。
/// </summary>
public record QqSingerStarNum : QqBaseModel
{
    [property: JsonPropertyName("singer_mid")]
    public string? SingerMid { get; set; }

    [property: JsonPropertyName("singer_name")]
    public string? SingerName { get; set; }

    [property: JsonPropertyName("fans")]
    public long? Fans { get; set; }

    [property: JsonPropertyName("num")]
    public long? Num { get; set; }

    [property: JsonPropertyName("starnum")]
    public long? StarNum { get; set; }

    [property: JsonPropertyName("star_num")]
    public long? StarNum2 { get; set; }

    [property: JsonPropertyName("blogflag")]
    public int? BlogFlag { get; set; }
}
