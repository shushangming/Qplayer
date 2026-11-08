using System.Text.Json;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Transport;

namespace QqMusic.Net.Protocol.Raw;

/// <summary>
///     搜索域 Raw API 层：直接构造 QqRequest 并通过 IQqTransport 发送，返回原始 JsonElement。
/// </summary>
public class RawSearchApi(IQqTransport transport)
{
    /// <summary>
    ///     综合搜索（remoteplace: song/album/mv/singer/playlist）。
    ///     GET /getSearchByKey?key&amp;limit&amp;page&amp;catZhida&amp;remoteplace
    /// </summary>
    public async Task<JsonElement> SearchAsync(string key, int limit = 20, int page = 1,
        string remoteplace = "song", string? catZhida = null)
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSearchByKey",
            Params = new Dictionary<string, string>
            {
                ["key"] = key,
                ["limit"] = limit.ToString(),
                ["page"] = page.ToString(),
                ["remoteplace"] = remoteplace
            }
        };

        if (!string.IsNullOrEmpty(catZhida))
            request.Params["catZhida"] = catZhida;

        return await transport.SendAsync(request);
    }

    /// <summary>
    ///     搜索联想（智能盒子）。
    ///     GET /getSmartbox?key
    /// </summary>
    public async Task<JsonElement> GetSmartboxAsync(string key)
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSmartbox",
            Params = new Dictionary<string, string>
            {
                ["key"] = key
            }
        };

        return await transport.SendAsync(request);
    }

    /// <summary>
    ///     热搜列表。
    ///     GET /getHotkey
    /// </summary>
    public async Task<JsonElement> GetHotKeyAsync()
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getHotkey"
        };

        return await transport.SendAsync(request);
    }
}
