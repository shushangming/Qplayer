using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Adapters.Common;
using QqMusic.Net.Protocol.Raw;
using QqMusic.Net.util;

namespace QqMusic.Net.Clients;

/// <summary>
///     搜索域 Client 层：基于 RawSearchApi，把 JsonElement 解析为强类型模型。
/// </summary>
public class SearchClient(RawSearchApi rawApi)
{
    /// <summary>
    ///     综合搜索，返回按域分区的强类型结果。
    /// </summary>
    public async Task<QqSearchResult?> SearchAsync(string key, int limit = 20, int page = 1,
        string remoteplace = "song", string? catZhida = null)
    {
        var json = await rawApi.SearchAsync(key, limit, page, remoteplace, catZhida);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqSearchResult);
    }

    /// <summary>
    ///     综合搜索，返回原始 JsonElement（不解析）。
    /// </summary>
    public async Task<JsonElement> SearchRawAsync(string key, int limit = 20, int page = 1,
        string remoteplace = "song", string? catZhida = null)
    {
        return await rawApi.SearchAsync(key, limit, page, remoteplace, catZhida);
    }

    /// <summary>
    ///     搜索联想（智能盒子）。
    /// </summary>
    public async Task<QqSmartboxResult?> GetSmartboxAsync(string key)
    {
        var json = await rawApi.GetSmartboxAsync(key);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqSmartboxResult);
    }

    /// <summary>
    ///     搜索联想，返回原始 JsonElement（不解析）。
    /// </summary>
    public async Task<JsonElement> GetSmartboxRawAsync(string key)
    {
        return await rawApi.GetSmartboxAsync(key);
    }

    /// <summary>
    ///     热搜列表。
    /// </summary>
    public async Task<QqHotKeyList?> GetHotKeyAsync()
    {
        var json = await rawApi.GetHotKeyAsync();
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqHotKeyList);
    }

    /// <summary>
    ///     热搜列表，返回原始 JsonElement（不解析）。
    /// </summary>
    public async Task<JsonElement> GetHotKeyRawAsync()
    {
        return await rawApi.GetHotKeyAsync();
    }
}
