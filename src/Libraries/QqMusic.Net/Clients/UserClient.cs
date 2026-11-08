using System.Linq;
using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Adapters.Common;
using QqMusic.Net.Protocol.Raw;
using QqMusic.Net.util;

namespace QqMusic.Net.Clients;

/// <summary>
///     用户域客户端。在 Raw API 之上做强类型反序列化。
/// </summary>
public class UserClient(RawUserApi rawApi)
{
    /// <summary>
    ///     用户歌单。
    /// </summary>
    public async Task<QqUserPlaylists?> GetUserPlaylistsAsync(long uin, int offset = 0, int limit = 30, string? cookie = null)
    {
        var json = await rawApi.GetUserPlaylistsAsync(uin, offset, limit, cookie);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqUserPlaylists);
    }

    /// <summary>
    ///     用户歌单（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetUserPlaylistsRawAsync(long uin, int offset = 0, int limit = 30, string? cookie = null)
    {
        return rawApi.GetUserPlaylistsAsync(uin, offset, limit, cookie);
    }

    /// <summary>
    ///     用户详情。
    /// </summary>
    public async Task<QqUserInfo?> GetUserDetailAsync(long uin, string? cookie = null)
    {
        var json = await rawApi.GetUserDetailAsync(uin, cookie);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqUserInfo);
    }

    /// <summary>
    ///     用户详情（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetUserDetailRawAsync(long uin, string? cookie = null)
    {
        return rawApi.GetUserDetailAsync(uin, cookie);
    }

    /// <summary>
    ///     用户收藏歌单。
    /// </summary>
    public async Task<QqUserCollectedPlaylists?> GetUserCollectedSongListsAsync(long uin, int page = 1, int limit = 30, string? cookie = null)
    {
        var json = await rawApi.GetUserCollectedSongListsAsync(uin, page, limit, cookie);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqUserCollectedPlaylists);
    }

    /// <summary>
    ///     用户收藏歌单（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetUserCollectedSongListsRawAsync(long uin, int page = 1, int limit = 30, string? cookie = null)
    {
        return rawApi.GetUserCollectedSongListsAsync(uin, page, limit, cookie);
    }

    /// <summary>
    ///     用户关注歌手。
    /// </summary>
    public async Task<QqUserFollowSingers?> GetUserFollowSingersAsync(string? cookie = null)
    {
        var json = await rawApi.GetUserFollowSingersAsync(cookie);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqUserFollowSingers);
    }

    /// <summary>
    ///     用户关注歌手（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetUserFollowSingersRawAsync(string? cookie = null)
    {
        return rawApi.GetUserFollowSingersAsync(cookie);
    }

    /// <summary>
    ///     用户头像（uin 或 k 二选一）。
    /// </summary>
    public async Task<QqUserAvatar?> GetUserAvatarAsync(long? uin = null, string? k = null, int size = 100)
    {
        var json = await rawApi.GetUserAvatarAsync(uin, k, size);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqUserAvatar);
    }

    /// <summary>
    ///     用户头像（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetUserAvatarRawAsync(long? uin = null, string? k = null, int size = 100)
    {
        return rawApi.GetUserAvatarAsync(uin, k, size);
    }

    /// <summary>
    ///     用户喜欢的音乐（需 cookie）。
    /// </summary>
    public async Task<QqUserLikedSongs?> GetUserLikedSongsAsync(string? cookie = null)
    {
        var json = await rawApi.GetUserLikedSongsAsync(cookie);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqUserLikedSongs);
    }

    /// <summary>
    ///     用户喜欢的音乐（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetUserLikedSongsRawAsync(string? cookie = null)
    {
        return rawApi.GetUserLikedSongsAsync(cookie);
    }

    /// <summary>
    ///     VIP 信息。
    /// </summary>
    public async Task<QqVipInfo?> GetVipInfoAsync(string? cookie = null)
    {
        var json = await rawApi.GetVipInfoAsync(cookie);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqVipInfo);
    }

    /// <summary>
    ///     VIP 信息（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetVipInfoRawAsync(string? cookie = null)
    {
        return rawApi.GetVipInfoAsync(cookie);
    }

    /// <summary>
    ///     获取当前 cookie。
    /// </summary>
    public async Task<QqCookieInfo?> GetCookieInfoAsync()
    {
        var json = await rawApi.GetCookieInfoAsync();
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqCookieInfo);
    }

    /// <summary>
    ///     获取当前 cookie（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetCookieInfoRawAsync()
    {
        return rawApi.GetCookieInfoAsync();
    }

    /// <summary>
    ///     设置 cookie。
    /// </summary>
    public async Task<QqCookieInfo?> SetCookieAsync(string cookie)
    {
        var json = await rawApi.SetCookieAsync(cookie);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqCookieInfo);
    }

    /// <summary>
    ///     设置 cookie（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> SetCookieRawAsync(string cookie)
    {
        return rawApi.SetCookieAsync(cookie);
    }

    /// <summary>
    ///     获取"我喜欢的音乐"歌单 disstid。
    ///     上游 /user/getUserLikedSongs 只用服务端配置文件 cookie（客户端传入无效），
    ///     因此直连 QQ 官方个人信息主页 fcg 解析 mymusic 中 title=我喜欢（type=1）条目的 id。
    /// </summary>
    /// <param name="uin">当前登录 QQ 数字 uin（从 cookie 中解析）。</param>
    /// <param name="cookie">客户端保存的完整 QQ 登录 cookie。</param>
    /// <returns>"我喜欢"歌单 disstid；未找到或失败返回 null。</returns>
    public async Task<long?> GetLikedPlaylistIdAsync(long uin, string? cookie)
    {
        if (uin <= 0 || string.IsNullOrWhiteSpace(cookie))
            return null;

        var json = await rawApi.GetProfileHomepageAsync(uin, cookie).ConfigureAwait(false);
        var profile = json.Deserialize(AppJsonContext.Default.QqProfileHomepage);
        var entries = profile?.Data?.Mymusic;
        if (entries is not { Count: > 0 })
            return null;

        // 优先 title 含"喜欢"，其次 type==1（QQ 的"我喜欢"入口 type 恒为 1）
        var liked = entries.FirstOrDefault(e =>
            (e.Title != null && e.Title.Contains("喜欢")) || e.Type == 1);
        return liked?.Id is > 0 ? liked.Id : null;
    }

    /// <summary>
    ///     获取"我喜欢的音乐"歌单 disstid（原始 JsonElement 版，供冒烟测试/诊断）。
    /// </summary>
    public Task<JsonElement> GetProfileHomepageRawAsync(long uin, string? cookie)
    {
        return rawApi.GetProfileHomepageAsync(uin, cookie);
    }
}
