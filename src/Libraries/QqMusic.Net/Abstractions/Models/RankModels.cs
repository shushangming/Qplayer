using System.Text.Json.Serialization;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     排行榜信息（getTopLists 返回的 topList 元素）。
///     真实字段：id/listenCount/picUrl/topTitle/type/songList。
/// </summary>
public record QqTopListInfo : QqBaseModel
{
    [property: JsonPropertyName("id")]
    public int? Id { get; set; }

    [property: JsonPropertyName("listenCount")]
    public long? ListenCount { get; set; }

    [property: JsonPropertyName("picUrl")]
    public string? PicUrl { get; set; }

    [property: JsonPropertyName("topTitle")]
    public string? TopTitle { get; set; }

    [property: JsonPropertyName("type")]
    public int? Type { get; set; }

    [property: JsonPropertyName("songList")]
    public List<QqTopListSongPreview>? SongList { get; set; }
}

/// <summary>
///     排行榜歌曲预览（topList 元素内嵌的 songList）。
/// </summary>
public record QqTopListSongPreview
{
    [property: JsonPropertyName("singername")]
    public string? SingerName { get; set; }

    [property: JsonPropertyName("songname")]
    public string? SongName { get; set; }
}

/// <summary>
///     排行榜列表结果（getTopLists）。
/// </summary>
public record QqTopList : QqBaseModel
{
    [property: JsonPropertyName("group")]
    public List<QqTopListGroup>? Groups { get; set; }

    [property: JsonPropertyName("topList")]
    public List<QqTopListInfo>? TopLists { get; set; }
}

/// <summary>
///     排行榜分组。
/// </summary>
public record QqTopListGroup
{
    [property: JsonPropertyName("groupName")]
    public string? GroupName { get; set; }

    [property: JsonPropertyName("opername")]
    public string? OperName { get; set; }

    [property: JsonPropertyName("toplist")]
    public List<QqTopListInfo>? TopLists { get; set; }
}

/// <summary>
///     排行榜歌曲（getRanks 返回的 song 数组元素）。
///     注意：排行榜接口字段为 camelCase（songId/albumMid/singerName），
///     与搜索接口的全小写（songid/albummid/singername）不同，因此独立建模。
/// </summary>
public record QqRankSong
{
    [property: JsonPropertyName("rank")]
    public int Rank { get; set; }

    [property: JsonPropertyName("rankType")]
    public int? RankType { get; set; }

    [property: JsonPropertyName("rankValue")]
    public string? RankValue { get; set; }

    [property: JsonPropertyName("recType")]
    public int? RecType { get; set; }

    [property: JsonPropertyName("songId")]
    public long? SongId { get; set; }

    [property: JsonPropertyName("vid")]
    public string? Vid { get; set; }

    [property: JsonPropertyName("albumMid")]
    public string? AlbumMid { get; set; }

    [property: JsonPropertyName("title")]
    public string? Title { get; set; }

    [property: JsonPropertyName("singerName")]
    public string? SingerName { get; set; }

    [property: JsonPropertyName("singerMid")]
    public string? SingerMid { get; set; }

    [property: JsonPropertyName("songType")]
    public int? SongType { get; set; }

    [property: JsonPropertyName("uuidCnt")]
    public int? UuidCnt { get; set; }

    [property: JsonPropertyName("cover")]
    public string? Cover { get; set; }

    [property: JsonPropertyName("mvid")]
    public int? MvId { get; set; }
}

/// <summary>
///     排行榜详情结果（getRanks，载荷在 response.req_1.data.data）。
/// </summary>
public record QqRankResult : QqBaseModel
{
    [property: JsonPropertyName("topId")]
    public int? TopId { get; set; }

    [property: JsonPropertyName("recType")]
    public int? RecType { get; set; }

    [property: JsonPropertyName("topType")]
    public int? TopType { get; set; }

    [property: JsonPropertyName("updateType")]
    public int? UpdateType { get; set; }

    [property: JsonPropertyName("title")]
    public string? Title { get; set; }

    [property: JsonPropertyName("titleDetail")]
    public string? TitleDetail { get; set; }

    [property: JsonPropertyName("titleShare")]
    public string? TitleShare { get; set; }

    [property: JsonPropertyName("titleSub")]
    public string? TitleSub { get; set; }

    [property: JsonPropertyName("intro")]
    public string? Intro { get; set; }

    [property: JsonPropertyName("period")]
    public string? Period { get; set; }

    [property: JsonPropertyName("updateTime")]
    public string? UpdateTime { get; set; }

    [property: JsonPropertyName("totalNum")]
    public int? TotalNum { get; set; }

    [property: JsonPropertyName("song")]
    public List<QqRankSong>? Songs { get; set; }

    [property: JsonPropertyName("topAlbumURL")]
    public string? TopAlbumUrl { get; set; }

    [property: JsonPropertyName("groupType")]
    public int? GroupType { get; set; }
}
