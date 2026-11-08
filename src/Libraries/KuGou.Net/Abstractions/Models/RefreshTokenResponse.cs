using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
///     刷新登录 Token 的结果。
/// </summary>
public record RefreshTokenResponse : KgBaseModel
{
    /// <summary>
    ///     用户 ID。
    /// </summary>
    [property: JsonPropertyName("userid")] public long UserId { get; set; }

    /// <summary>
    ///     刷新后的 Token。
    /// </summary>
    [property: JsonPropertyName("token")] public string Token { get; set; } = string.Empty;

    /// <summary>
    ///     是否为 VIP。
    /// </summary>
    [property: JsonPropertyName("is_vip")] public long IsVip { get; set; }

    /// <summary>
    ///     刷新后返回的 `t1` 凭证。
    /// </summary>
    [property: JsonPropertyName("t1")] public string T1 { get; set; } = string.Empty;
}
