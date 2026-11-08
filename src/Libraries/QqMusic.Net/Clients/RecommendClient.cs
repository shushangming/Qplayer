using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Adapters.Common;
using QqMusic.Net.Protocol.Raw;
using QqMusic.Net.util;

namespace QqMusic.Net.Clients;

/// <summary>
///     推荐域 Client：调用 <see cref="RawRecommendApi"/> 并把 <see cref="JsonElement"/> 解析为强类型模型。
/// </summary>
public class RecommendClient(RawRecommendApi rawApi)
{
    /// <summary>
    ///     每日推荐（需登录）。
    /// </summary>
    public async Task<QqDailyRecommend?> GetDailyRecommendAsync(string? cookie = null)
    {
        var json = await rawApi.GetDailyRecommendAsync(cookie);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqDailyRecommend);
    }

    /// <summary>
    ///     每日推荐（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetDailyRecommendRawAsync(string? cookie = null)
    {
        return rawApi.GetDailyRecommendAsync(cookie);
    }

    /// <summary>
    ///     私人 FM（需登录）。
    /// </summary>
    public async Task<QqPrivateFm?> GetPrivateFmAsync(string? cookie = null)
    {
        var json = await rawApi.GetPrivateFmAsync(cookie);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqPrivateFm);
    }

    /// <summary>
    ///     私人 FM（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetPrivateFmRawAsync(string? cookie = null)
    {
        return rawApi.GetPrivateFmAsync(cookie);
    }

    /// <summary>
    ///     新歌速递。真实结构：response.new_song.data.{lanlist, lan, songlist}
    /// </summary>
    public async Task<QqNewSongs?> GetNewSongsAsync(int? areaId = null, int? limit = null)
    {
        var json = await rawApi.GetNewSongsAsync(areaId, limit);
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var data = QqApiResponseParser.Drill(payload, "new_song", "data");
        return data.Deserialize(AppJsonContext.Default.QqNewSongs);
    }

    /// <summary>
    ///     新歌速递（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetNewSongsRawAsync(int? areaId = null, int? limit = null)
    {
        return rawApi.GetNewSongsAsync(areaId, limit);
    }

    /// <summary>
    ///     首页推荐包。真实结构：response.category.data.{category:[...]}
    /// </summary>
    public async Task<QqRecommend?> GetRecommendAsync()
    {
        var json = await rawApi.GetRecommendAsync();
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var data = QqApiResponseParser.Drill(payload, "category", "data");
        return data.Deserialize(AppJsonContext.Default.QqRecommend);
    }

    /// <summary>
    ///     首页推荐包（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetRecommendRawAsync()
    {
        return rawApi.GetRecommendAsync();
    }

    /// <summary>
    ///     推荐 Banner。真实结构：response.focus.data.{content:[...]}
    /// </summary>
    public async Task<QqRecommendBanner?> GetRecommendBannerAsync()
    {
        var json = await rawApi.GetRecommendBannerAsync();
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var data = QqApiResponseParser.Drill(payload, "focus", "data");
        return data.Deserialize(AppJsonContext.Default.QqRecommendBanner);
    }

    /// <summary>
    ///     推荐 Banner（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetRecommendBannerRawAsync()
    {
        return rawApi.GetRecommendBannerAsync();
    }

    /// <summary>
    ///     个性化推荐（需登录）。
    /// </summary>
    public async Task<QqPersonalRecommend?> GetPersonalRecommendAsync(string? type = null, string? cookie = null)
    {
        var json = await rawApi.GetPersonalRecommendAsync(type, cookie);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqPersonalRecommend);
    }

    /// <summary>
    ///     个性化推荐（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetPersonalRecommendRawAsync(string? type = null, string? cookie = null)
    {
        return rawApi.GetPersonalRecommendAsync(type, cookie);
    }

    /// <summary>
    ///     相似歌曲（需登录，未登录时上游返回错误码）。
    /// </summary>
    public async Task<QqSimilarSongs?> GetSimilarSongsAsync(string? songmid = null, string? cookie = null)
    {
        var json = await rawApi.GetSimilarSongsAsync(songmid, cookie);
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        // 真实结构可能为 response.similarSong.data（登录时）或 response.similarSong.{code}（未登录）
        var data = QqApiResponseParser.Drill(payload, "similarSong", "data");
        return data.Deserialize(AppJsonContext.Default.QqSimilarSongs);
    }

    /// <summary>
    ///     相似歌曲（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetSimilarSongsRawAsync(string? songmid = null, string? cookie = null)
    {
        return rawApi.GetSimilarSongsAsync(songmid, cookie);
    }

    /// <summary>
    ///     相关歌单。真实结构：response.gedan.data.{vec_gedan, has_more}
    /// </summary>
    public async Task<QqRelatedPlaylists?> GetRelatedPlaylistsAsync(
        long? songid = null,
        string? sin = null,
        string? lastId = null,
        int? songType = null)
    {
        var json = await rawApi.GetRelatedPlaylistsAsync(songid, sin, lastId, songType);
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var data = QqApiResponseParser.Drill(payload, "gedan", "data");
        return data.Deserialize(AppJsonContext.Default.QqRelatedPlaylists);
    }

    /// <summary>
    ///     相关歌单（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetRelatedPlaylistsRawAsync(
        long? songid = null,
        string? sin = null,
        string? lastId = null,
        int? songType = null)
    {
        return rawApi.GetRelatedPlaylistsAsync(songid, sin, lastId, songType);
    }
}
