using System.Text.Json.Serialization.Metadata;

namespace QqMusic.Net.Protocol.Transport;

/// <summary>
///     QQ 音乐 API 请求描述。
/// </summary>
public class QqRequest
{
    /// <summary>
    ///     HTTP 方法，默认 GET。
    /// </summary>
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    /// <summary>
    ///     API 路径，例如 "/getSearchByKey"。
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    ///     Query 参数。
    /// </summary>
    public Dictionary<string, string> Params { get; set; } = new();

    /// <summary>
    ///     POST Body 对象（序列化为 JSON）。
    /// </summary>
    public object? Body { get; set; }

    /// <summary>
    ///     POST Body 的 JsonTypeInfo（用于 AOT 源生成序列化）。
    /// </summary>
    public JsonTypeInfo? BodyTypeInfo { get; set; }

    /// <summary>
    ///     覆盖 BaseUrl（如不指定则使用默认 localhost:3200）。
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    ///     自定义请求头。
    /// </summary>
    public Dictionary<string, string>? CustomHeaders { get; set; }
}
