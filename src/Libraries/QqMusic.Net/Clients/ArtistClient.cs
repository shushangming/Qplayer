using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Adapters.Common;
using QqMusic.Net.Protocol.Raw;
using QqMusic.Net.util;

namespace QqMusic.Net.Clients;

/// <summary>
///     歌手（singer）域客户端。在 Raw API 之上做强类型反序列化。
/// </summary>
public class ArtistClient(RawArtistApi rawApi)
{
    /// <summary>
    ///     歌手列表。真实结构：response.singerList.data.{singerlist, total, area, genre, index, sex}
    ///     index 传数字（-100=热门，上游 Number(index)），不可传 "ALL"。
    /// </summary>
    public async Task<QqSingerList?> GetSingerListAsync(
        int area = -100,
        int sex = -100,
        int genre = -100,
        int index = -100,
        int page = 1)
    {
        var json = await rawApi.GetSingerListAsync(area, sex, genre, index, page);
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var data = QqApiResponseParser.Drill(payload, "singerList", "data");
        return data.Deserialize(AppJsonContext.Default.QqSingerList);
    }

    /// <summary>
    ///     歌手列表（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetSingerListRawAsync(
        int area = -100,
        int sex = -100,
        int genre = -100,
        int index = -100,
        int page = 1)
    {
        return rawApi.GetSingerListAsync(area, sex, genre, index, page);
    }

    /// <summary>
    ///     歌手分类。真实结构：response.singerList.data.{area, genre, index, sex, singerlist, total}
    /// </summary>
    public async Task<QqSingerCategory?> GetSingerCategoryAsync()
    {
        var json = await rawApi.GetSingerCategoryAsync();
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var data = QqApiResponseParser.Drill(payload, "singerList", "data");
        return data.Deserialize(AppJsonContext.Default.QqSingerCategory);
    }

    /// <summary>
    ///     歌手分类（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetSingerCategoryRawAsync()
    {
        return rawApi.GetSingerCategoryAsync();
    }

    /// <summary>
    ///     歌手简介。注意：该接口返回 XML 字符串（response 为 string），
    ///     此处提取 XML 中的 desc 字段文本；若解析失败则返回原始内容。
    /// </summary>
    public async Task<QqSingerDesc?> GetSingerDescAsync(string singermid)
    {
        var json = await rawApi.GetSingerDescAsync(singermid);
        // 解析器解出 response；getSingerDesc 的 response 是 XML 字符串
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        if (payload.ValueKind == JsonValueKind.String)
        {
            var xml = payload.GetString() ?? "";
            // 简单提取 <desc><![CDATA[...]]></desc>
            var desc = ExtractCdata(xml, "desc");
            var info = ExtractCdata(xml, "info");
            var idStr = ExtractTag(xml, "id");
            long? singerId = long.TryParse(idStr, out var id) ? id : null;
            return new QqSingerDesc { SingerId = singerId, SingerMid = singermid, Desc = desc, Info = info };
        }
        return payload.Deserialize(AppJsonContext.Default.QqSingerDesc);
    }

    /// <summary>
    ///     歌手简介（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetSingerDescRawAsync(string singermid)
    {
        return rawApi.GetSingerDescAsync(singermid);
    }

    /// <summary>
    ///     歌手热门歌曲。真实结构：response.singer.data.{songlist}
    /// </summary>
    public async Task<QqSingerHotSongs?> GetSingerHotsongAsync(string singermid, int limit = 30, int page = 1)
    {
        var json = await rawApi.GetSingerHotsongAsync(singermid, limit, page);
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var data = QqApiResponseParser.Drill(payload, "singer", "data");
        return data.Deserialize(AppJsonContext.Default.QqSingerHotSongs);
    }

    /// <summary>
    ///     歌手热门歌曲（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetSingerHotsongRawAsync(string singermid, int limit = 30, int page = 1)
    {
        return rawApi.GetSingerHotsongAsync(singermid, limit, page);
    }

    /// <summary>
    ///     歌手专辑。真实结构：response.singer.data.{singerMid, albumList}
    /// </summary>
    public async Task<QqSingerAlbums?> GetSingerAlbumAsync(string singermid, int limit = 30, int page = 1)
    {
        var json = await rawApi.GetSingerAlbumAsync(singermid, limit, page);
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var data = QqApiResponseParser.Drill(payload, "singer", "data");
        return data.Deserialize(AppJsonContext.Default.QqSingerAlbums);
    }

    /// <summary>
    ///     歌手专辑（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetSingerAlbumRawAsync(string singermid, int limit = 30, int page = 1)
    {
        return rawApi.GetSingerAlbumAsync(singermid, limit, page);
    }

    /// <summary>
    ///     歌手 MV。真实结构：response.data.{list}（该接口有 response.data 封装）。
    /// </summary>
    public async Task<QqSingerMvs?> GetSingerMvAsync(string singermid, string order = "time", int num = 30)
    {
        var json = await rawApi.GetSingerMvAsync(singermid, order, num);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqSingerMvs);
    }

    /// <summary>
    ///     歌手 MV（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetSingerMvRawAsync(string singermid, string order = "time", int num = 30)
    {
        return rawApi.GetSingerMvAsync(singermid, order, num);
    }

    /// <summary>
    ///     相似歌手。
    /// </summary>
    public async Task<QqSimilarSingers?> GetSimilarSingerAsync(string singermid)
    {
        var json = await rawApi.GetSimilarSingerAsync(singermid);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqSimilarSingers);
    }

    /// <summary>
    ///     相似歌手（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetSimilarSingerRawAsync(string singermid)
    {
        return rawApi.GetSimilarSingerAsync(singermid);
    }

    /// <summary>
    ///     歌手星数（人气）。真实结构：response.{code, num, status}（无 data 封装）。
    /// </summary>
    public async Task<QqSingerStarNum?> GetSingerStarNumAsync(string singermid)
    {
        var json = await rawApi.GetSingerStarNumAsync(singermid);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqSingerStarNum);
    }

    /// <summary>
    ///     歌手星数（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetSingerStarNumRawAsync(string singermid)
    {
        return rawApi.GetSingerStarNumAsync(singermid);
    }

    private static string? ExtractCdata(string xml, string tag)
    {
        var open = $"<{tag}>";
        var close = $"</{tag}>";
        var start = xml.IndexOf(open, StringComparison.Ordinal);
        if (start < 0) return null;
        start += open.Length;
        var end = xml.IndexOf(close, start, StringComparison.Ordinal);
        if (end < 0) return null;
        var content = xml.Substring(start, end - start);
        // 去掉 <![CDATA[ 和 ]]>
        const string cdataOpen = "<![CDATA[";
        const string cdataClose = "]]>";
        if (content.StartsWith(cdataOpen)) content = content.Substring(cdataOpen.Length);
        if (content.EndsWith(cdataClose)) content = content.Substring(0, content.Length - cdataClose.Length);
        return content;
    }

    private static string? ExtractTag(string xml, string tag)
    {
        var open = $"<{tag}>";
        var close = $"</{tag}>";
        var start = xml.IndexOf(open, StringComparison.Ordinal);
        if (start < 0) return null;
        start += open.Length;
        var end = xml.IndexOf(close, start, StringComparison.Ordinal);
        return end < 0 ? null : xml.Substring(start, end - start);
    }
}
