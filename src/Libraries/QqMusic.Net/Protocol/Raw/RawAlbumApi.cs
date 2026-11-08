using System.Text.Json;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Transport;

namespace QqMusic.Net.Protocol.Raw;

/// <summary>
///     专辑域 Raw API：直接构建 <see cref="QqRequest" /> 并返回原始 <see cref="JsonElement" />。
///     GET 使用 Params，无签名，BaseUrl 使用默认值。
/// </summary>
public class RawAlbumApi(IQqTransport transport)
{
    /// <summary>
    ///     GET /getAlbumSongs —— 专辑歌曲列表（albummid 与 albumid 二选一）。
    /// </summary>
    public Task<JsonElement> GetAlbumSongsAsync(string? albummid = null, long? albumid = null, int? begin = null,
        int? limit = null, int? order = null)
    {
        var query = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(albummid)) query["albummid"] = albummid;
        if (albumid.HasValue) query["albumid"] = albumid.Value.ToString();
        if (begin.HasValue) query["begin"] = begin.Value.ToString();
        if (limit.HasValue) query["limit"] = limit.Value.ToString();
        if (order.HasValue) query["order"] = order.Value.ToString();

        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getAlbumSongs",
            Params = query
        };
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getAlbumInfo —— 专辑信息。
    /// </summary>
    public Task<JsonElement> GetAlbumInfoAsync(string albummid)
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getAlbumInfo",
            Params = new Dictionary<string, string> { ["albummid"] = albummid }
        };
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getNewDisks —— 新碟上架。
    /// </summary>
    public Task<JsonElement> GetNewDisksAsync(int? page = null, int? limit = null)
    {
        var query = new Dictionary<string, string>();
        if (page.HasValue) query["page"] = page.Value.ToString();
        if (limit.HasValue) query["limit"] = limit.Value.ToString();

        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getNewDisks",
            Params = query
        };
        return transport.SendAsync(request);
    }
}
