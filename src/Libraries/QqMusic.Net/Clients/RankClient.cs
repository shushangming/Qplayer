using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Adapters.Common;
using QqMusic.Net.Protocol.Raw;
using QqMusic.Net.util;

namespace QqMusic.Net.Clients;

/// <summary>
///     排行榜域客户端：调用 <see cref="RawRankApi" /> 并用 <see cref="QqApiResponseParser" /> 反序列化为强类型模型。
///     每个方法都有对应的 Raw 版本（位于 <see cref="RawRankApi" />）直接返回原始 JSON。
/// </summary>
public class RankClient(RawRankApi rawApi)
{
    /// <summary>
    ///     GET /getTopLists —— 排行榜列表（榜单元数据）。
    /// </summary>
    public async Task<QqTopList?> GetTopListsAsync()
    {
        var json = await rawApi.GetTopListsAsync().ConfigureAwait(false);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqTopList);
    }

    /// <summary>
    ///     GET /getRanks —— 排行榜详情。
    ///     真实结构为 response.req_1.data.data，需在解析后继续钻取。
    /// </summary>
    public async Task<QqRankResult?> GetRanksAsync(int? topId = null, int? page = null, int? limit = null,
        int? resolveMid = null)
    {
        var json = await rawApi.GetRanksAsync(topId, page, limit, resolveMid).ConfigureAwait(false);
        // 解析器解出 response（getRanks 无 response.data，载荷在 req_1.data.data）
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        // 钻取 req_1 -> data -> data
        var rankData = QqApiResponseParser.Drill(payload, "req_1", "data", "data");
        return rankData.Deserialize(AppJsonContext.Default.QqRankResult);
    }

    /// <summary>
    ///     GET /getRanks —— 排行榜详情，返回原始 JsonElement。
    /// </summary>
    public async Task<JsonElement> GetRanksRawAsync(int? topId = null, int? page = null, int? limit = null,
        int? resolveMid = null)
    {
        return await rawApi.GetRanksAsync(topId, page, limit, resolveMid).ConfigureAwait(false);
    }

    /// <summary>
    ///     GET /getTopLists —— 排行榜列表，返回原始 JsonElement。
    /// </summary>
    public async Task<JsonElement> GetTopListsRawAsync()
    {
        return await rawApi.GetTopListsAsync().ConfigureAwait(false);
    }
}
