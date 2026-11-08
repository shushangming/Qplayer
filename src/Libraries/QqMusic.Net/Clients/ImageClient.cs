using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Adapters.Common;
using QqMusic.Net.Protocol.Raw;
using QqMusic.Net.util;

namespace QqMusic.Net.Clients;

/// <summary>
///     图片/杂项域 Client：调用 <see cref="RawImageApi"/> 并把 <see cref="JsonElement"/> 解析为强类型模型。
/// </summary>
public class ImageClient(RawImageApi rawApi)
{
    /// <summary>
    ///     图片 URL（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetImageUrlRawAsync(string id, string? size = null, int? maxAge = null)
    {
        return rawApi.GetImageUrlAsync(id, size, maxAge);
    }

    /// <summary>
    ///     图片 URL（size：150x150 / 300x300 / 500x500 / 800x800）。
    /// </summary>
    public async Task<QqImageUrl?> GetImageUrlAsync(string id, string? size = null, int? maxAge = null)
    {
        var json = await rawApi.GetImageUrlAsync(id, size, maxAge);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqImageUrl);
    }

    /// <summary>
    ///     下载 QQ 音乐客户端信息（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetDownloadQQMusicRawAsync()
    {
        return rawApi.GetDownloadQQMusicAsync();
    }

    /// <summary>
    ///     下载 QQ 音乐客户端信息。
    /// </summary>
    public async Task<QqDownloadInfo?> GetDownloadQQMusicAsync()
    {
        var json = await rawApi.GetDownloadQQMusicAsync();
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqDownloadInfo);
    }

    /// <summary>
    ///     票据信息（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetTicketInfoRawAsync()
    {
        return rawApi.GetTicketInfoAsync();
    }

    /// <summary>
    ///     票据信息。
    /// </summary>
    public async Task<QqTicketInfo?> GetTicketInfoAsync()
    {
        var json = await rawApi.GetTicketInfoAsync();
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqTicketInfo);
    }
}
