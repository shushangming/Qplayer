using System.Text.Json.Serialization;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     QQ 音乐歌曲信息。
/// </summary>
public record QqSongInfo : QqBaseModel
{
    [property: JsonPropertyName("songmid")]
    public string SongMid { get; set; } = "";

    [property: JsonPropertyName("songid")]
    public long? SongId { get; set; }

    [property: JsonPropertyName("songname")]
    public string SongName { get; set; } = "";

    [property: JsonPropertyName("songname_hilight")]
    public string? SongNameHilight { get; set; }

    [property: JsonPropertyName("singer")]
    public List<QqSingerLite>? Singers { get; set; }

    [property: JsonPropertyName("albummid")]
    public string? AlbumMid { get; set; }

    [property: JsonPropertyName("albumid")]
    public long? AlbumId { get; set; }

    [property: JsonPropertyName("albumname")]
    public string? AlbumName { get; set; }

    [property: JsonPropertyName("albumname_hilight")]
    public string? AlbumNameHilight { get; set; }

    [property: JsonPropertyName("interval")]
    public int Interval { get; set; }

    [property: JsonPropertyName("strMediaMid")]
    public string? StrMediaMid { get; set; }

    [property: JsonPropertyName("media_mid")]
    public string? MediaMid { get; set; }

    [property: JsonPropertyName("size128")]
    public long? Size128 { get; set; }

    [property: JsonPropertyName("size320")]
    public long? Size320 { get; set; }

    [property: JsonPropertyName("sizeflac")]
    public long? SizeFlac { get; set; }

    [property: JsonPropertyName("sizeape")]
    public long? SizeApe { get; set; }

    [property: JsonPropertyName("sizeogg")]
    public long? SizeOgg { get; set; }

    [property: JsonPropertyName("pubtime")]
    public long? PubTime { get; set; }

    [property: JsonPropertyName("vid")]
    public string? Vid { get; set; }

    [property: JsonPropertyName("type")]
    public int? Type { get; set; }

    [property: JsonPropertyName("tag")]
    public int? Tag { get; set; }

    [property: JsonPropertyName("stream")]
    public int? Stream { get; set; }

    [property: JsonPropertyName("pay")]
    public QqSongPayInfo? Pay { get; set; }

    [property: JsonPropertyName("action")]
    public QqSongActionInfo? Action { get; set; }

    [property: JsonPropertyName("ksong")]
    public QqSongKsongInfo? Ksong { get; set; }

    [property: JsonPropertyName("volume")]
    public QqSongVolumeInfo? Volume { get; set; }

    [property: JsonPropertyName("lyric")]
    public string? Lyric { get; set; }

    [property: JsonPropertyName("lyric_hilight")]
    public string? LyricHilight { get; set; }

    [property: JsonPropertyName("t")]
    public int? T { get; set; }

    /// <summary>
    ///     封面图 URL（通过 albummid 拼接）。
    /// </summary>
    [JsonIgnore]
    public string? CoverUrl => string.IsNullOrEmpty(AlbumMid)
        ? null
        : $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{AlbumMid}.jpg";
}

/// <summary>
///     歌手简要信息。
/// </summary>
public record QqSingerLite
{
    [property: JsonPropertyName("id")]
    public long? Id { get; set; }

    [property: JsonPropertyName("mid")]
    public string Mid { get; set; } = "";

    [property: JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [property: JsonPropertyName("name_hilight")]
    public string? NameHilight { get; set; }

    [property: JsonPropertyName("type")]
    public int? Type { get; set; }

    [property: JsonPropertyName("uin")]
    public long? Uin { get; set; }
}

/// <summary>
///     歌曲付费信息。
/// </summary>
public record QqSongPayInfo
{
    [property: JsonPropertyName("payalbum")]
    public int? PayAlbum { get; set; }

    [property: JsonPropertyName("payalbumprice")]
    public int? PayAlbumPrice { get; set; }

    [property: JsonPropertyName("paydownload")]
    public int? PayDownload { get; set; }

    [property: JsonPropertyName("payinfo")]
    public int? PayInfo { get; set; }

    [property: JsonPropertyName("payplay")]
    public int? PayPlay { get; set; }

    [property: JsonPropertyName("paytrackmouth")]
    public int? PayTrackMouth { get; set; }

    [property: JsonPropertyName("paytrackprice")]
    public int? PayTrackPrice { get; set; }

    [property: JsonPropertyName("timefree")]
    public int? TimeFree { get; set; }
}

/// <summary>
///     歌曲操作权限信息。
/// </summary>
public record QqSongActionInfo
{
    [property: JsonPropertyName("alert")]
    public int? Alert { get; set; }

    [property: JsonPropertyName("icons")]
    public int? Icons { get; set; }

    [property: JsonPropertyName("msgdown")]
    public int? MsgDown { get; set; }

    [property: JsonPropertyName("msgfav")]
    public int? MsgFav { get; set; }

    [property: JsonPropertyName("msgid")]
    public int? MsgId { get; set; }

    [property: JsonPropertyName("msgpay")]
    public int? MsgPay { get; set; }

    [property: JsonPropertyName("msgshare")]
    public int? MsgShare { get; set; }

    [property: JsonPropertyName("switch")]
    public int? Switch { get; set; }
}

/// <summary>
///     歌曲 K 歌信息。
/// </summary>
public record QqSongKsongInfo
{
    [property: JsonPropertyName("id")]
    public long? Id { get; set; }

    [property: JsonPropertyName("mid")]
    public string? Mid { get; set; }
}

/// <summary>
///     歌曲音量信息。
/// </summary>
public record QqSongVolumeInfo
{
    [property: JsonPropertyName("gain")]
    public double? Gain { get; set; }

    [property: JsonPropertyName("lra")]
    public double? Lra { get; set; }

    [property: JsonPropertyName("peak")]
    public double? Peak { get; set; }
}

/// <summary>
///     歌曲播放地址结果（getMusicPlay）。
///     真实结构：data.playUrl.{songmid:{url, error?}}——playUrl 是按 songmid 为键的对象。
/// </summary>
public record QqSongPlayResult : QqBaseModel
{
    /// <summary>
    ///     播放地址字典（键为 songmid）。
    /// </summary>
    [property: JsonPropertyName("playUrl")]
    public Dictionary<string, QqPlayUrlItem>? PlayUrls { get; set; }

    /// <summary>
    ///     是否成功获取播放地址（任一 songmid 下有非空 url）。
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => PlayUrls != null && PlayUrls.Values.Any(x => !string.IsNullOrWhiteSpace(x.Url));

    /// <summary>
    ///     取指定 songmid 的播放地址；不存在时取第一个可用地址。
    /// </summary>
    public string? GetPlayableUrl(string songmid)
    {
        if (PlayUrls == null) return null;
        if (PlayUrls.TryGetValue(songmid, out var item) && !string.IsNullOrWhiteSpace(item.Url))
            return item.Url;
        return PlayUrls.Values
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Url))?.Url;
    }
}

/// <summary>
///     单个歌曲的播放地址项。
/// </summary>
public record QqPlayUrlItem
{
    [property: JsonPropertyName("url")]
    public string? Url { get; set; }

    [property: JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
///     批量获取歌曲信息请求体。
/// </summary>
public record QqBatchSongInfoRequest
{
    [property: JsonPropertyName("songs")]
    public List<List<string>> Songs { get; set; } = new();
}
