using System.Text.Json.Serialization;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     图片 URL 结果。
/// </summary>
public record QqImageUrl : QqBaseModel
{
    [property: JsonPropertyName("id")]
    public string? Id { get; set; }

    [property: JsonPropertyName("url")]
    public string? Url { get; set; }

    [property: JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [property: JsonPropertyName("size")]
    public string? Size { get; set; }

    [property: JsonPropertyName("maxAge")]
    public int? MaxAge { get; set; }
}

/// <summary>
///     下载 QQ 音乐客户端信息。
/// </summary>
public record QqDownloadInfo : QqBaseModel
{
    [property: JsonPropertyName("url")]
    public string? Url { get; set; }

    [property: JsonPropertyName("version")]
    public string? Version { get; set; }

    [property: JsonPropertyName("size")]
    public string? Size { get; set; }
}

/// <summary>
///     票据信息。
/// </summary>
public record QqTicketInfo : QqBaseModel
{
    [property: JsonPropertyName("ticket")]
    public string? Ticket { get; set; }

    [property: JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }
}
