using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using QqMusic.Net.Abstractions.Models;

namespace QqMusic.Net.Adapters.Common;

/// <summary>
///     QQ 音乐 API 响应解析器。
///     真实封装格式（qq-music-api Node 服务）：
///     1. 多数接口：{ "response": { "code": 0, "data": { 实际载荷 } } }
///     2. 部分接口：{ "response": { "code": 0, "&lt;domainKey&gt;": { "code": 0, "data": { ... } } } }
///        （如 getRanks→req_1、getNewDisks→new_album、getSingerCategory→singerList）
///     3. 扁平接口：{ "response": { "retcode": 0, "code": 0, "lyric": {...} } }（如 getLyric）
///     解析器自动解出 response.data；无 data 时返回 response 本身，由 Client 层继续钻取。
/// </summary>
public static class QqApiResponseParser
{
    /// <summary>
    ///     解析响应：提取 payload -> 反序列化 -> 回填外层状态。
    /// </summary>
    public static T? Parse<T>(JsonElement root, JsonTypeInfo<T> typeInfo)
    {
        // 1. 捕获根级状态（如果有）
        int? rootStatus = null;
        int? rootCode = null;
        string? rootMessage = null;

        if (root.TryGetProperty("status", out var s) && s.ValueKind == JsonValueKind.Number)
            rootStatus = s.GetInt32();

        if (root.TryGetProperty("code", out var c) && c.ValueKind == JsonValueKind.Number)
            rootCode = c.GetInt32();

        if (root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
            rootMessage = m.GetString();

        // 2. 提取 response 层
        var response = ExtractResponse(root);

        // 3. 从 response 层捕获 code/message/subcode（真实接口的状态在 response 里）
        if (response.TryGetProperty("code", out var rc) && rc.ValueKind == JsonValueKind.Number)
            rootCode ??= rc.GetInt32();

        if (response.TryGetProperty("message", out var rm) && rm.ValueKind == JsonValueKind.String)
            rootMessage ??= rm.GetString();

        // 4. 如果 response 下有 data 对象，自动钻取（多数接口的载荷在 response.data）
        var targetElement = response;
        if (response.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
            targetElement = data;

        if (typeof(T) == typeof(JsonElement))
            return (T)(object)targetElement;

        // 健壮性：载荷为字符串/数字/null（如错误消息）时不抛异常，返回默认值
        if (targetElement.ValueKind != JsonValueKind.Object &&
            targetElement.ValueKind != JsonValueKind.Array)
            return default;

        var result = targetElement.Deserialize(typeInfo);

        // 5. 回填外层状态到基类
        if (result is QqBaseModel baseModel)
        {
            if (baseModel.Status == null && rootStatus.HasValue)
                baseModel.Status = rootStatus;

            if (baseModel.Code == null && rootCode.HasValue)
                baseModel.Code = rootCode;

            if (baseModel.Message == null && rootMessage != null)
                baseModel.Message = rootMessage;
        }

        return result;
    }

    /// <summary>
    ///     从根元素提取 response 对象。
    ///     支持 {body:{response}} 和 {response} 两种格式。
    /// </summary>
    private static JsonElement ExtractResponse(JsonElement root)
    {
        if (root.TryGetProperty("body", out var body) && body.ValueKind == JsonValueKind.Object)
        {
            if (body.TryGetProperty("response", out var response1) && response1.ValueKind != JsonValueKind.Null)
                return response1;
            return body;
        }

        if (root.TryGetProperty("response", out var response2) && response2.ValueKind != JsonValueKind.Null)
            return response2;

        return root;
    }

    /// <summary>
    ///     从已解析的 JsonElement 中按路径钻取（用于 response 下无 data、
    ///     而载荷在 domainKey.data.data 等嵌套结构的接口，如 getRanks）。
    /// </summary>
    public static JsonElement Drill(JsonElement element, params string[] path)
    {
        var current = element;
        foreach (var key in path)
        {
            if (current.ValueKind != JsonValueKind.Object)
                return current;
            if (!current.TryGetProperty(key, out var next))
                return current;
            current = next;
        }
        return current;
    }
}
