using System.Text.Json;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Transport;

namespace QqMusic.Net.Protocol.Raw;

/// <summary>
///     歌手（QQ API 中称 singer）域原始 API。路径参数使用 singermid。
///     BaseUrl 默认 localhost:3200，GET 拼 query，无签名。
/// </summary>
public class RawArtistApi(IQqTransport transport)
{
    /// <summary>
    ///     GET /getSingerList?area&amp;sex&amp;genre&amp;index&amp;page — 歌手列表。
    ///     注意：index 必须传数字（上游 Number(index)，-100=热门），传 "ALL" 会得到 NaN 空列表。
    /// </summary>
    public Task<JsonElement> GetSingerListAsync(
        int area = -100,
        int sex = -100,
        int genre = -100,
        int index = -100,
        int page = 1)
    {
        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSingerList",
            Params = new Dictionary<string, string>
            {
                ["area"] = area.ToString(),
                ["sex"] = sex.ToString(),
                ["genre"] = genre.ToString(),
                ["index"] = index.ToString(),
                ["page"] = page.ToString()
            }
        });
    }

    /// <summary>
    ///     GET /getSingerCategory — 歌手分类。
    /// </summary>
    public Task<JsonElement> GetSingerCategoryAsync()
    {
        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSingerCategory"
        });
    }

    /// <summary>
    ///     GET /getSingerDesc?singermid — 歌手简介。
    /// </summary>
    public Task<JsonElement> GetSingerDescAsync(string singermid)
    {
        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSingerDesc",
            Params = new Dictionary<string, string>
            {
                ["singermid"] = singermid
            }
        });
    }

    /// <summary>
    ///     GET /getSingerHotsong?singermid&amp;limit&amp;page — 歌手热门歌曲。
    /// </summary>
    public Task<JsonElement> GetSingerHotsongAsync(string singermid, int limit = 30, int page = 1)
    {
        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSingerHotsong",
            Params = new Dictionary<string, string>
            {
                ["singermid"] = singermid,
                ["limit"] = limit.ToString(),
                ["page"] = page.ToString()
            }
        });
    }

    /// <summary>
    ///     GET /getSingerAlbum?singermid&amp;limit&amp;page — 歌手专辑。
    /// </summary>
    public Task<JsonElement> GetSingerAlbumAsync(string singermid, int limit = 30, int page = 1)
    {
        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSingerAlbum",
            Params = new Dictionary<string, string>
            {
                ["singermid"] = singermid,
                ["limit"] = limit.ToString(),
                ["page"] = page.ToString()
            }
        });
    }

    /// <summary>
    ///     GET /getSingerMv?singermid&amp;order&amp;num — 歌手 MV。
    /// </summary>
    public Task<JsonElement> GetSingerMvAsync(string singermid, string order = "time", int num = 30)
    {
        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSingerMv",
            Params = new Dictionary<string, string>
            {
                ["singermid"] = singermid,
                ["order"] = order,
                ["num"] = num.ToString()
            }
        });
    }

    /// <summary>
    ///     GET /getSimilarSinger?singermid — 相似歌手。
    /// </summary>
    public Task<JsonElement> GetSimilarSingerAsync(string singermid)
    {
        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSimilarSinger",
            Params = new Dictionary<string, string>
            {
                ["singermid"] = singermid
            }
        });
    }

    /// <summary>
    ///     GET /getSingerStarNum?singermid — 歌手星数（人气）。
    /// </summary>
    public Task<JsonElement> GetSingerStarNumAsync(string singermid)
    {
        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSingerStarNum",
            Params = new Dictionary<string, string>
            {
                ["singermid"] = singermid
            }
        });
    }
}
