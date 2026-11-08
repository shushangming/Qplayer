using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Adapters.Common;
using QqMusic.Net.Protocol.Raw;
using QqMusic.Net.util;

namespace QqMusic.Net.Clients;

/// <summary>
///     MV 域 Client：调用 <see cref="RawMvApi"/> 并把 <see cref="JsonElement"/> 解析为强类型模型。
/// </summary>
public class MvClient(RawMvApi rawApi)
{
    /// <summary>
    ///     MV 播放地址。真实结构：response.getMVUrl.data.{vid:{mp4:[...]}}
    /// </summary>
    public async Task<QqMvPlay?> GetMvPlayAsync(string vid)
    {
        var json = await rawApi.GetMvPlayAsync(vid);
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var data = QqApiResponseParser.Drill(payload, "getMVUrl", "data");
        // data 下以 vid 为 key，取第一个对象的 mp4 列表
        foreach (var prop in data.EnumerateObject())
        {
            if (prop.Value.ValueKind == JsonValueKind.Object &&
                prop.Value.TryGetProperty("mp4", out var mp4) &&
                mp4.ValueKind == JsonValueKind.Array)
            {
                var urls = mp4.Deserialize(AppJsonContext.Default.ListQqMvPlayUrl);
                return new QqMvPlay { Vid = prop.Name, Mp4Urls = urls };
            }
        }
        return new QqMvPlay { Vid = vid };
    }

    /// <summary>
    ///     MV 播放地址（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetMvPlayRawAsync(string vid)
    {
        return rawApi.GetMvPlayAsync(vid);
    }

    /// <summary>
    ///     MV 列表。真实结构：response.mv_list.data.{list, total}
    /// </summary>
    public async Task<QqMvList?> GetMvAsync(
        int? area_id = null,
        int? version_id = null,
        int? limit = null,
        int? page = null)
    {
        var json = await rawApi.GetMvAsync(area_id, version_id, limit, page);
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var data = QqApiResponseParser.Drill(payload, "mv_list", "data");
        return data.Deserialize(AppJsonContext.Default.QqMvList);
    }

    /// <summary>
    ///     MV 列表（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetMvRawAsync(
        int? area_id = null,
        int? version_id = null,
        int? limit = null,
        int? page = null)
    {
        return rawApi.GetMvAsync(area_id, version_id, limit, page);
    }

    /// <summary>
    ///     MV 分类。真实结构：response.mv_tag.data.{area, version}
    /// </summary>
    public async Task<QqMvCategory?> GetMvCategoryAsync()
    {
        var json = await rawApi.GetMvCategoryAsync();
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var data = QqApiResponseParser.Drill(payload, "mv_tag", "data");
        return data.Deserialize(AppJsonContext.Default.QqMvCategory);
    }

    /// <summary>
    ///     MV 分类（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetMvCategoryRawAsync()
    {
        return rawApi.GetMvCategoryAsync();
    }

    /// <summary>
    ///     按标签获取 MV。真实结构：response.data.{lan, mvlist}（解析器自动解 response.data）。
    /// </summary>
    public async Task<QqMvByTag?> GetMvByTagAsync(string? tag = null)
    {
        var json = await rawApi.GetMvByTagAsync(tag);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqMvByTag);
    }

    /// <summary>
    ///     按标签获取 MV（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetMvByTagRawAsync(string? tag = null)
    {
        return rawApi.GetMvByTagAsync(tag);
    }
}
