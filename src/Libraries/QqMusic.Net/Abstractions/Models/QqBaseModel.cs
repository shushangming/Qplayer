using System.Text.Json;
using System.Text.Json.Serialization;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     所有 QQ 音乐模型的基类，自动捕获多余字段。
///     QQ 音乐 API 外层状态语义为 status=200 表示成功。
/// </summary>
public abstract record QqBaseModel
{
    /// <summary>
    ///     外层状态码（QQ 音乐为 200 表示成功）。
    /// </summary>
    [property: JsonPropertyName("status")]
    public int? Status { get; set; }

    /// <summary>
    ///     错误码（如果有）。
    /// </summary>
    [JsonPropertyName("code")]
    public int? Code { get; set; }

    /// <summary>
    ///     错误消息（如果有）。
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    ///     捕获未映射的额外字段。
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extras { get; set; }

    /// <summary>
    ///     从 Extras 里取字符串值。
    /// </summary>
    public string? GetExtraString(string key)
    {
        if (Extras != null && Extras.TryGetValue(key, out var val))
            return val.ValueKind == JsonValueKind.String ? val.GetString() : val.ToString();
        return null;
    }
}
