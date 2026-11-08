using System.Text.Json;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Transport;

namespace QqMusic.Net.Protocol.Raw;

/// <summary>
///     排行榜域 Raw API：直接构建 <see cref="QqRequest" /> 并返回原始 <see cref="JsonElement" />。
///     GET 使用 Params，无签名，BaseUrl 使用默认值。
/// </summary>
public class RawRankApi(IQqTransport transport)
{
    /// <summary>
    ///     GET /getTopLists —— 排行榜列表（榜单元数据）。
    /// </summary>
    public Task<JsonElement> GetTopListsAsync()
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getTopLists"
        };
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getRanks —— 排行榜详情。
    /// </summary>
    public Task<JsonElement> GetRanksAsync(int? topId = null, int? page = null, int? limit = null,
        int? resolveMid = null)
    {
        var query = new Dictionary<string, string>();
        if (topId.HasValue) query["topId"] = topId.Value.ToString();
        if (page.HasValue) query["page"] = page.Value.ToString();
        if (limit.HasValue) query["limit"] = limit.Value.ToString();
        if (resolveMid.HasValue) query["resolveMid"] = resolveMid.Value.ToString();

        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getRanks",
            Params = query
        };
        return transport.SendAsync(request);
    }
}
