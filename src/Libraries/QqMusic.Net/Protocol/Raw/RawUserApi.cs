using System.Text.Json;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Transport;

namespace QqMusic.Net.Protocol.Raw;

/// <summary>
///     用户域原始 API。所有路径均位于 /user/ 下。BaseUrl 默认 localhost:3200，GET 拼 query，无签名。
/// </summary>
public class RawUserApi(IQqTransport transport)
{
    /// <summary>
    ///     GET /user/getUserPlaylists?uin&amp;offset&amp;limit&amp;cookie — 用户歌单。
    /// </summary>
    public Task<JsonElement> GetUserPlaylistsAsync(long uin, int offset = 0, int limit = 30, string? cookie = null)
    {
        var p = new Dictionary<string, string>
        {
            ["uin"] = uin.ToString(),
            ["offset"] = offset.ToString(),
            ["limit"] = limit.ToString()
        };
        if (!string.IsNullOrEmpty(cookie)) p["cookie"] = cookie;

        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/user/getUserPlaylists",
            Params = p
        });
    }

    /// <summary>
    ///     GET /user/getUserDetail?uin&amp;cookie — 用户详情。
    /// </summary>
    public Task<JsonElement> GetUserDetailAsync(long uin, string? cookie = null)
    {
        var p = new Dictionary<string, string>
        {
            ["uin"] = uin.ToString()
        };
        if (!string.IsNullOrEmpty(cookie)) p["cookie"] = cookie;

        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/user/getUserDetail",
            Params = p
        });
    }

    /// <summary>
    ///     GET /user/getUserCollectedSongLists?uin&amp;page&amp;limit&amp;cookie — 用户收藏歌单。
    /// </summary>
    public Task<JsonElement> GetUserCollectedSongListsAsync(long uin, int page = 1, int limit = 30, string? cookie = null)
    {
        var p = new Dictionary<string, string>
        {
            ["uin"] = uin.ToString(),
            ["page"] = page.ToString(),
            ["limit"] = limit.ToString()
        };
        if (!string.IsNullOrEmpty(cookie)) p["cookie"] = cookie;

        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/user/getUserCollectedSongLists",
            Params = p
        });
    }

    /// <summary>
    ///     GET /user/getUserFollowSingers — 用户关注歌手。
    /// </summary>
    public Task<JsonElement> GetUserFollowSingersAsync(string? cookie = null)
    {
        var p = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(cookie)) p["cookie"] = cookie;

        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/user/getUserFollowSingers",
            Params = p
        });
    }

    /// <summary>
    ///     GET /user/getUserAvatar?uin|k&amp;size — 用户头像（uin 或 k 二选一）。
    /// </summary>
    public Task<JsonElement> GetUserAvatarAsync(long? uin = null, string? k = null, int size = 100)
    {
        var p = new Dictionary<string, string>
        {
            ["size"] = size.ToString()
        };
        if (uin.HasValue) p["uin"] = uin.Value.ToString();
        if (!string.IsNullOrEmpty(k)) p["k"] = k;

        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/user/getUserAvatar",
            Params = p
        });
    }

    /// <summary>
    ///     GET /user/getUserLikedSongs — 用户喜欢的音乐（需 cookie）。
    /// </summary>
    public Task<JsonElement> GetUserLikedSongsAsync(string? cookie = null)
    {
        var p = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(cookie)) p["cookie"] = cookie;

        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/user/getUserLikedSongs",
            Params = p
        });
    }

    /// <summary>
    ///     GET /user/getVipInfo?cookie — VIP 信息。
    /// </summary>
    public Task<JsonElement> GetVipInfoAsync(string? cookie = null)
    {
        var p = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(cookie)) p["cookie"] = cookie;

        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/user/getVipInfo",
            Params = p
        });
    }

    /// <summary>
    ///     GET /user/getCookie — 获取当前 cookie。
    /// </summary>
    public Task<JsonElement> GetCookieInfoAsync()
    {
        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/user/getCookie"
        });
    }

    /// <summary>
    ///     GET /user/setCookie?cookie — 设置 cookie。
    /// </summary>
    public Task<JsonElement> SetCookieAsync(string cookie)
    {
        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/user/setCookie",
            Params = new Dictionary<string, string>
            {
                ["cookie"] = cookie
            }
        });
    }

    /// <summary>
    ///     直连 QQ 官方个人信息主页 fcg 接口（c6.y.qq.com），获取用户歌单/喜欢/收藏等入口信息。
    ///     ⚠ 上游 qq-music-api 的 /user/getUserLikedSongs 只用服务端配置文件 cookie（客户端传入的 cookie 被忽略），
    ///     因此"我喜欢"歌单的 disstid 只能通过本方法直连官方接口获取（带客户端 cookie + Referer，无签名）。
    ///     返回顶层 JsonElement（含 data.mymusic[]，其中 title=我喜欢 条目的 id 即"我喜欢"歌单 disstid）。
    /// </summary>
    public Task<JsonElement> GetProfileHomepageAsync(long uin, string? cookie)
    {
        var p = new Dictionary<string, string>
        {
            ["cv"] = "4747474",
            ["ct"] = "24",
            ["format"] = "json",
            ["inCharset"] = "utf-8",
            ["outCharset"] = "utf-8",
            ["notice"] = "0",
            ["platform"] = "yqq.json",
            ["needNewCode"] = "0",
            ["uin"] = uin.ToString(),
            ["g_tk_new_20200303"] = "0",
            ["g_tk"] = "0",
            ["cid"] = "205360838",
            ["userid"] = uin.ToString(),
            ["reqfrom"] = "1",
            ["reqtype"] = "0",
            ["hostUin"] = "0",
            ["loginUin"] = uin.ToString()
        };

        var headers = new Dictionary<string, string>
        {
            ["Referer"] = $"https://y.qq.com/portal/profile.html?uin={uin}"
        };
        if (!string.IsNullOrWhiteSpace(cookie))
            headers["Cookie"] = cookie;

        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            BaseUrl = "https://c6.y.qq.com",
            Path = "/rsc/fcgi-bin/fcg_get_profile_homepage.fcg",
            Params = p,
            CustomHeaders = headers
        });
    }
}
