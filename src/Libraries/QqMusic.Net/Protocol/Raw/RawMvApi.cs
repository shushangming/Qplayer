using System.Text.Json;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Transport;

namespace QqMusic.Net.Protocol.Raw;

/// <summary>
///     MV 域 Raw API：构建 <see cref="QqRequest"/> 并返回原始 <see cref="JsonElement"/>。
/// </summary>
public class RawMvApi(IQqTransport transport)
{
    /// <summary>
    ///     GET /getMvPlay — MV 播放地址。
    /// </summary>
    /// <param name="vid">MV vid。</param>
    public Task<JsonElement> GetMvPlayAsync(string vid)
    {
        var request = new QqRequest
        {
            Path = "/getMvPlay",
            Params = new Dictionary<string, string>
            {
                ["vid"] = vid
            }
        };
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getMv — MV 列表。
    /// </summary>
    /// <param name="area_id">可选，地区 ID。</param>
    /// <param name="version_id">可选，版本 ID。</param>
    /// <param name="limit">可选，返回条数。</param>
    /// <param name="page">可选，页码。</param>
    public Task<JsonElement> GetMvAsync(
        int? area_id = null,
        int? version_id = null,
        int? limit = null,
        int? page = null)
    {
        var request = new QqRequest
        {
            Path = "/getMv",
            Params = new Dictionary<string, string>()
        };
        if (area_id.HasValue)
            request.Params["area_id"] = area_id.Value.ToString();
        if (version_id.HasValue)
            request.Params["version_id"] = version_id.Value.ToString();
        if (limit.HasValue)
            request.Params["limit"] = limit.Value.ToString();
        if (page.HasValue)
            request.Params["page"] = page.Value.ToString();
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getMvCategory — MV 分类。
    /// </summary>
    public Task<JsonElement> GetMvCategoryAsync()
    {
        var request = new QqRequest
        {
            Path = "/getMvCategory"
        };
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getMvByTag — 按标签获取 MV。
    /// </summary>
    /// <param name="tag">可选，标签；不传则返回默认结果。</param>
    public Task<JsonElement> GetMvByTagAsync(string? tag = null)
    {
        var request = new QqRequest
        {
            Path = "/getMvByTag",
            Params = new Dictionary<string, string>()
        };
        if (!string.IsNullOrWhiteSpace(tag))
            request.Params["tag"] = tag;
        return transport.SendAsync(request);
    }
}
