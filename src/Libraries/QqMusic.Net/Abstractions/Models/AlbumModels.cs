using System.Text.Json.Serialization;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     专辑简要信息。
/// </summary>
public record QqAlbumLite
{
    [property: JsonPropertyName("albumID")]
    public long? AlbumId { get; set; }

    [property: JsonPropertyName("albumMID")]
    public string AlbumMid { get; set; } = "";

    [property: JsonPropertyName("albumName")]
    public string? AlbumName { get; set; }

    [property: JsonPropertyName("album_name")]
    public string? AlbumName2 { get; set; }

    [property: JsonPropertyName("albumPic")]
    public string? AlbumPic { get; set; }

    [property: JsonPropertyName("album_pic")]
    public string? AlbumPic2 { get; set; }

    [property: JsonPropertyName("singerID")]
    public long? SingerId { get; set; }

    [property: JsonPropertyName("singerMID")]
    public string? SingerMid { get; set; }

    [property: JsonPropertyName("singerName")]
    public string? SingerName { get; set; }

    [property: JsonPropertyName("singer_name")]
    public string? SingerName2 { get; set; }

    [property: JsonPropertyName("publicTime")]
    public string? PublicTime { get; set; }

    [property: JsonPropertyName("public_time")]
    public string? PublicTime2 { get; set; }

    [property: JsonPropertyName("songCount")]
    public int? SongCount { get; set; }

    [property: JsonPropertyName("song_count")]
    public int? SongCount2 { get; set; }

    // 歌手专辑等接口使用的字段
    [property: JsonPropertyName("albumTranName")]
    public string? AlbumTranName { get; set; }

    [property: JsonPropertyName("publishDate")]
    public string? PublishDate { get; set; }

    [property: JsonPropertyName("albumType")]
    public string? AlbumType { get; set; }

    [property: JsonPropertyName("pmid")]
    public string? Pmid { get; set; }

    [property: JsonPropertyName("totalNum")]
    public int? TotalNum { get; set; }

    [property: JsonPropertyName("tags")]
    public string? Tags { get; set; }

    // 新碟上架等接口使用的新风格字段
    [property: JsonPropertyName("id")]
    public long? Id { get; set; }

    [property: JsonPropertyName("mid")]
    public string? Mid { get; set; }

    [property: JsonPropertyName("name")]
    public string? Name { get; set; }

    [property: JsonPropertyName("trans_name")]
    public string? TransName { get; set; }

    [property: JsonPropertyName("singers")]
    public List<QqSingerLite>? Singers { get; set; }

    [property: JsonPropertyName("release_time")]
    public string? ReleaseTime { get; set; }
}

/// <summary>
///     专辑详情（载荷在 response.data 下）。
/// </summary>
public record QqAlbumInfo : QqBaseModel
{
    [property: JsonPropertyName("albumID")]
    public long? AlbumId { get; set; }

    [property: JsonPropertyName("albumMID")]
    public string? AlbumMid { get; set; }

    [property: JsonPropertyName("albumName")]
    public string? AlbumName { get; set; }

    [property: JsonPropertyName("albumPic")]
    public string? AlbumPic { get; set; }

    [property: JsonPropertyName("albumDesc")]
    public string? AlbumDesc { get; set; }

    [property: JsonPropertyName("album_desc")]
    public string? AlbumDesc2 { get; set; }

    [property: JsonPropertyName("singerID")]
    public long? SingerId { get; set; }

    [property: JsonPropertyName("singerMID")]
    public string? SingerMid { get; set; }

    [property: JsonPropertyName("singerName")]
    public string? SingerName { get; set; }

    [property: JsonPropertyName("publicTime")]
    public string? PublicTime { get; set; }

    [property: JsonPropertyName("songCount")]
    public int? SongCount { get; set; }

    [property: JsonPropertyName("genre")]
    public string? Genre { get; set; }

    [property: JsonPropertyName("language")]
    public string? Language { get; set; }

    // getAlbumInfo 实测返回字段
    [property: JsonPropertyName("aDate")]
    public string? ADate { get; set; }

    [property: JsonPropertyName("albumTips")]
    public string? AlbumTips { get; set; }

    [property: JsonPropertyName("company")]
    public string? Company { get; set; }

    [property: JsonPropertyName("headPic")]
    public string? HeadPic { get; set; }

    [property: JsonPropertyName("cur_song_num")]
    public int? CurSongNum { get; set; }

    [property: JsonPropertyName("desc")]
    public string? Desc { get; set; }

    [property: JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>
///     专辑歌曲列表结果。
/// </summary>
public record QqAlbumSongs : QqBaseModel
{
    [property: JsonPropertyName("albumID")]
    public long? AlbumId { get; set; }

    [property: JsonPropertyName("albumMID")]
    public string? AlbumMid { get; set; }

    [property: JsonPropertyName("albumName")]
    public string? AlbumName { get; set; }

    [property: JsonPropertyName("list")]
    public List<QqSongInfo>? Songs { get; set; }

    [property: JsonPropertyName("songList")]
    public List<QqSongInfo>? SongList { get; set; }

    [property: JsonPropertyName("totalNum")]
    public int? TotalNum { get; set; }

    [property: JsonPropertyName("curNum")]
    public int? CurNum { get; set; }
}

/// <summary>
///     新碟上架结果（载荷在 response.new_album.data 下）。
/// </summary>
public record QqNewDisks : QqBaseModel
{
    [property: JsonPropertyName("albums")]
    public List<QqAlbumLite>? Albums { get; set; }

    [property: JsonPropertyName("list")]
    public List<QqAlbumLite>? List { get; set; }

    [property: JsonPropertyName("total")]
    public int? Total { get; set; }

    [property: JsonPropertyName("ret_msg")]
    public string? RetMsg { get; set; }
}
