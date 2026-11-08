using System.Text.Json.Serialization;

namespace KuGou.Net.Abstractions.Models;

/// <summary>
///     登录二维码信息。
/// </summary>
public record QRCode : KgBaseModel
{
    /// <summary>
    ///     二维码文本内容。
    /// </summary>
    [JsonPropertyName("qrcode")] public string Qrcode { get; set; } = string.Empty;

    /// <summary>
    ///     二维码图片地址。
    /// </summary>
    [JsonPropertyName("qrcode_img")] public string QrcodeImg { get; set; } = string.Empty;
}
