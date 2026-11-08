using System.Text.Json;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Transport;

namespace QqMusic.Net.Protocol.Raw;

/// <summary>
///     推荐域 Raw API：构建 <see cref="QqRequest"/> 并返回原始 <see cref="JsonElement"/>。
///     BaseUrl 默认 localhost:3200，GET 拼 query，无签名。
/// </summary>
public class RawRecommendApi(IQqTransport transport)
{
    /// <summary>
    ///     GET /getDailyRecommend — 每日推荐。
    /// </summary>
    /// <param name="cookie">可选，登录 cookie。</param>
    public Task<JsonElement> GetDailyRecommendAsync(string? cookie = null)
    {
        var request = new QqRequest
        {
            Path = "/getDailyRecommend",
            Params = new Dictionary<string, string>()
        };
        if (!string.IsNullOrWhiteSpace(cookie))
            request.Params["cookie"] = cookie;
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getPrivateFM — 私人 FM。
    /// </summary>
    /// <param name="cookie">可选，登录 cookie。</param>
    public Task<JsonElement> GetPrivateFmAsync(string? cookie = null)
    {
        var request = new QqRequest
        {
            Path = "/getPrivateFM",
            Params = new Dictionary<string, string>()
        };
        if (!string.IsNullOrWhiteSpace(cookie))
            request.Params["cookie"] = cookie;
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getNewSongs — 新歌速递。
    /// </summary>
    /// <param name="areaId">可选，地区 ID。</param>
    /// <param name="limit">可选，返回条数。</param>
    public Task<JsonElement> GetNewSongsAsync(int? areaId = null, int? limit = null)
    {
        var request = new QqRequest
        {
            Path = "/getNewSongs",
            Params = new Dictionary<string, string>()
        };
        if (areaId.HasValue)
            request.Params["areaId"] = areaId.Value.ToString();
        if (limit.HasValue)
            request.Params["limit"] = limit.Value.ToString();
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getRecommend — 首页推荐包。
    /// </summary>
    public Task<JsonElement> GetRecommendAsync()
    {
        var request = new QqRequest
        {
            Path = "/getRecommend"
        };
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getRecommendBanner — 推荐 Banner。
    /// </summary>
    public Task<JsonElement> GetRecommendBannerAsync()
    {
        var request = new QqRequest
        {
            Path = "/getRecommendBanner"
        };
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getPersonalRecommend — 个性化推荐。
    /// </summary>
    /// <param name="type">可选，推荐类型。</param>
    /// <param name="cookie">可选，登录 cookie。</param>
    public Task<JsonElement> GetPersonalRecommendAsync(string? type = null, string? cookie = null)
    {
        var request = new QqRequest
        {
            Path = "/getPersonalRecommend",
            Params = new Dictionary<string, string>()
        };
        if (!string.IsNullOrWhiteSpace(type))
            request.Params["type"] = type;
        if (!string.IsNullOrWhiteSpace(cookie))
            request.Params["cookie"] = cookie;
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getSimilarSongs — 相似歌曲。
    /// </summary>
    /// <param name="songmid">可选，歌曲 mid。</param>
    /// <param name="cookie">可选，登录 cookie。</param>
    public Task<JsonElement> GetSimilarSongsAsync(string? songmid = null, string? cookie = null)
    {
        var request = new QqRequest
        {
            Path = "/getSimilarSongs",
            Params = new Dictionary<string, string>()
        };
        if (!string.IsNullOrWhiteSpace(songmid))
            request.Params["songmid"] = songmid;
        if (!string.IsNullOrWhiteSpace(cookie))
            request.Params["cookie"] = cookie;
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getRelatedPlaylists — 相关歌单。
    /// </summary>
    /// <param name="songid">可选，歌曲 ID。</param>
    /// <param name="sin">可选，关联标识。</param>
    /// <param name="lastId">可选，分页游标。</param>
    /// <param name="songType">可选，歌曲类型。</param>
    public Task<JsonElement> GetRelatedPlaylistsAsync(
        long? songid = null,
        string? sin = null,
        string? lastId = null,
        int? songType = null)
    {
        var request = new QqRequest
        {
            Path = "/getRelatedPlaylists",
            Params = new Dictionary<string, string>()
        };
        if (songid.HasValue)
            request.Params["songid"] = songid.Value.ToString();
        if (!string.IsNullOrWhiteSpace(sin))
            request.Params["sin"] = sin;
        if (!string.IsNullOrWhiteSpace(lastId))
            request.Params["lastId"] = lastId;
        if (songType.HasValue)
            request.Params["songType"] = songType.Value.ToString();
        return transport.SendAsync(request);
    }
}
