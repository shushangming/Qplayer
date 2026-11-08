using System.Text.Json.Serialization;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     搜索结果（按域分区）。载荷在 response.data 下。
/// </summary>
public record QqSearchResult : QqBaseModel
{
    [property: JsonPropertyName("keyword")]
    public string? Keyword { get; set; }

    [property: JsonPropertyName("priority")]
    public int? Priority { get; set; }

    [property: JsonPropertyName("tab")]
    public int? Tab { get; set; }

    [property: JsonPropertyName("totaltime")]
    public int? TotalTime { get; set; }

    [property: JsonPropertyName("song")]
    public QqSearchSongZone? Song { get; set; }

    [property: JsonPropertyName("album")]
    public QqSearchAlbumZone? Album { get; set; }

    [property: JsonPropertyName("singer")]
    public QqSearchSingerZone? Singer { get; set; }

    [property: JsonPropertyName("playlist")]
    public QqSearchPlaylistZone? Playlist { get; set; }

    [property: JsonPropertyName("mv")]
    public QqSearchMvZone? Mv { get; set; }

    [property: JsonPropertyName("lyric")]
    public QqSearchLyricZone? Lyric { get; set; }

    [property: JsonPropertyName("semantic")]
    public QqSearchSongZone? Semantic { get; set; }

    [property: JsonPropertyName("zhida")]
    public QqSearchZhida? Zhida { get; set; }
}

/// <summary>
///     搜索歌曲分区。
/// </summary>
public record QqSearchSongZone
{
    [property: JsonPropertyName("curnum")]
    public int? CurNum { get; set; }

    [property: JsonPropertyName("curpage")]
    public int? CurPage { get; set; }

    [property: JsonPropertyName("list")]
    public List<QqSongInfo>? List { get; set; }

    [property: JsonPropertyName("totalnum")]
    public int? TotalNum { get; set; }

    [property: JsonPropertyName("totalpage")]
    public int? TotalPage { get; set; }
}

/// <summary>
///     搜索专辑分区。
/// </summary>
public record QqSearchAlbumZone
{
    [property: JsonPropertyName("curnum")]
    public int? CurNum { get; set; }

    [property: JsonPropertyName("curpage")]
    public int? CurPage { get; set; }

    [property: JsonPropertyName("list")]
    public List<QqAlbumLite>? List { get; set; }

    [property: JsonPropertyName("totalnum")]
    public int? TotalNum { get; set; }

    [property: JsonPropertyName("totalpage")]
    public int? TotalPage { get; set; }
}

/// <summary>
///     搜索歌手分区。
/// </summary>
public record QqSearchSingerZone
{
    [property: JsonPropertyName("curnum")]
    public int? CurNum { get; set; }

    [property: JsonPropertyName("curpage")]
    public int? CurPage { get; set; }

    [property: JsonPropertyName("list")]
    public List<QqSingerInfo>? List { get; set; }

    [property: JsonPropertyName("totalnum")]
    public int? TotalNum { get; set; }

    [property: JsonPropertyName("totalpage")]
    public int? TotalPage { get; set; }
}

/// <summary>
///     搜索歌单分区。
/// </summary>
public record QqSearchPlaylistZone
{
    [property: JsonPropertyName("curnum")]
    public int? CurNum { get; set; }

    [property: JsonPropertyName("curpage")]
    public int? CurPage { get; set; }

    [property: JsonPropertyName("list")]
    public List<QqPlaylistLite>? List { get; set; }

    [property: JsonPropertyName("totalnum")]
    public int? TotalNum { get; set; }

    [property: JsonPropertyName("totalpage")]
    public int? TotalPage { get; set; }
}

/// <summary>
///     搜索 MV 分区。
/// </summary>
public record QqSearchMvZone
{
    [property: JsonPropertyName("curnum")]
    public int? CurNum { get; set; }

    [property: JsonPropertyName("curpage")]
    public int? CurPage { get; set; }

    [property: JsonPropertyName("list")]
    public List<QqMvLite>? List { get; set; }

    [property: JsonPropertyName("totalnum")]
    public int? TotalNum { get; set; }

    [property: JsonPropertyName("totalpage")]
    public int? TotalPage { get; set; }
}

/// <summary>
///     搜索歌词分区。
/// </summary>
public record QqSearchLyricZone
{
    [property: JsonPropertyName("curnum")]
    public int? CurNum { get; set; }

    [property: JsonPropertyName("curpage")]
    public int? CurPage { get; set; }

    [property: JsonPropertyName("list")]
    public List<QqLyricSearchItem>? List { get; set; }

    [property: JsonPropertyName("totalnum")]
    public int? TotalNum { get; set; }

    [property: JsonPropertyName("totalpage")]
    public int? TotalPage { get; set; }
}

/// <summary>
///     搜索直达（智达）信息。真实结构为 {type, zhida_singer:{...}}。
/// </summary>
public record QqSearchZhida
{
    [property: JsonPropertyName("type")]
    public int? Type { get; set; }

    [property: JsonPropertyName("zhida_singer")]
    public QqZhidaSinger? ZhidaSinger { get; set; }
}

/// <summary>
///     搜索直达歌手信息。
/// </summary>
public record QqZhidaSinger
{
    [property: JsonPropertyName("singerID")]
    public long? SingerId { get; set; }

    [property: JsonPropertyName("singerMID")]
    public string? SingerMid { get; set; }

    [property: JsonPropertyName("singerName")]
    public string? SingerName { get; set; }

    [property: JsonPropertyName("singerPic")]
    public string? SingerPic { get; set; }

    [property: JsonPropertyName("songNum")]
    public int? SongNum { get; set; }

    [property: JsonPropertyName("albumNum")]
    public int? AlbumNum { get; set; }

    [property: JsonPropertyName("mvNum")]
    public int? MvNum { get; set; }

    [property: JsonPropertyName("hotsong")]
    public List<QqZhidaSong>? HotSongs { get; set; }

    [property: JsonPropertyName("hotalbum")]
    public List<QqZhidaAlbum>? HotAlbums { get; set; }
}

/// <summary>
///     直达热门歌曲。
/// </summary>
public record QqZhidaSong
{
    [property: JsonPropertyName("songID")]
    public long? SongId { get; set; }

    [property: JsonPropertyName("songMID")]
    public string? SongMid { get; set; }

    [property: JsonPropertyName("songName")]
    public string? SongName { get; set; }

    [property: JsonPropertyName("songname_hilight")]
    public string? SongNameHilight { get; set; }

    [property: JsonPropertyName("f")]
    public string? F { get; set; }
}

/// <summary>
///     直达热门专辑。
/// </summary>
public record QqZhidaAlbum
{
    [property: JsonPropertyName("albumID")]
    public long? AlbumId { get; set; }

    [property: JsonPropertyName("albumMID")]
    public string? AlbumMid { get; set; }

    [property: JsonPropertyName("albumName")]
    public string? AlbumName { get; set; }

    [property: JsonPropertyName("albumname_hilight")]
    public string? AlbumNameHilight { get; set; }
}

/// <summary>
///     歌词搜索结果项。
/// </summary>
public record QqLyricSearchItem
{
    [property: JsonPropertyName("songmid")]
    public string? SongMid { get; set; }

    [property: JsonPropertyName("songname")]
    public string? SongName { get; set; }

    [property: JsonPropertyName("singer")]
    public List<QqSingerLite>? Singers { get; set; }

    [property: JsonPropertyName("content")]
    public string? Content { get; set; }

    [property: JsonPropertyName("cnt")]
    public int? Cnt { get; set; }
}

/// <summary>
///     热搜关键词。真实字段为 k（关键词）、n（热度）。
/// </summary>
public record QqHotKey
{
    [property: JsonPropertyName("k")]
    public string? K { get; set; }

    [property: JsonPropertyName("n")]
    public long? N { get; set; }
}

/// <summary>
///     热搜列表结果。载荷在 response.data 下。
/// </summary>
public record QqHotKeyList : QqBaseModel
{
    [property: JsonPropertyName("hotkey")]
    public List<QqHotKey>? HotKeys { get; set; }

    [property: JsonPropertyName("special_key")]
    public string? SpecialKey { get; set; }

    [property: JsonPropertyName("special_url")]
    public string? SpecialUrl { get; set; }
}

/// <summary>
///     搜索联想（智能盒子）结果。载荷在 response.data 下。
///     真实结构为 {song:{count,itemlist}, singer:{...}, album:{...}, mv:{...}}。
/// </summary>
public record QqSmartboxResult : QqBaseModel
{
    [property: JsonPropertyName("song")]
    public QqSmartboxZone? Song { get; set; }

    [property: JsonPropertyName("singer")]
    public QqSmartboxZone? Singer { get; set; }

    [property: JsonPropertyName("album")]
    public QqSmartboxZone? Album { get; set; }

    [property: JsonPropertyName("mv")]
    public QqSmartboxZone? Mv { get; set; }
}

/// <summary>
///     搜索联想分区。
/// </summary>
public record QqSmartboxZone
{
    [property: JsonPropertyName("count")]
    public int? Count { get; set; }

    [property: JsonPropertyName("name")]
    public string? Name { get; set; }

    [property: JsonPropertyName("order")]
    public int? Order { get; set; }

    [property: JsonPropertyName("type")]
    public int? Type { get; set; }

    [property: JsonPropertyName("itemlist")]
    public List<QqSmartboxItem>? ItemList { get; set; }
}

/// <summary>
///     搜索联想项。
/// </summary>
public record QqSmartboxItem
{
    [property: JsonPropertyName("docid")]
    public string? DocId { get; set; }

    [property: JsonPropertyName("id")]
    public string? Id { get; set; }

    [property: JsonPropertyName("mid")]
    public string? Mid { get; set; }

    [property: JsonPropertyName("name")]
    public string? Name { get; set; }

    [property: JsonPropertyName("singer")]
    public string? Singer { get; set; }

    [property: JsonPropertyName("pic")]
    public string? Pic { get; set; }

    [property: JsonPropertyName("vid")]
    public string? Vid { get; set; }
}
