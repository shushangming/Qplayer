using System.Text.Json;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Transport;

namespace QqMusic.Net.Protocol.Raw;

/// <summary>
///     图片/杂项域 Raw API：构建 <see cref="QqRequest"/> 并返回原始 <see cref="JsonElement"/>。
/// </summary>
public class RawImageApi(IQqTransport transport)
{
    /// <summary>
    ///     GET /getImageUrl — 图片 URL。
    /// </summary>
    /// <param name="id">图片 ID（必填）。</param>
    /// <param name="size">可选，尺寸：150x150 / 300x300 / 500x500 / 800x800。</param>
    /// <param name="maxAge">可选，缓存最大时长（秒）。</param>
    public Task<JsonElement> GetImageUrlAsync(string id, string? size = null, int? maxAge = null)
    {
        var request = new QqRequest
        {
            Path = "/getImageUrl",
            Params = new Dictionary<string, string>
            {
                ["id"] = id
            }
        };
        if (!string.IsNullOrWhiteSpace(size))
            request.Params["size"] = size;
        if (maxAge.HasValue)
            request.Params["maxAge"] = maxAge.Value.ToString();
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getDownloadQQMusic — 下载 QQ 音乐客户端信息。
    /// </summary>
    public Task<JsonElement> GetDownloadQQMusicAsync()
    {
        var request = new QqRequest
        {
            Path = "/getDownloadQQMusic"
        };
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getTicketInfo — 票据信息。
    /// </summary>
    public Task<JsonElement> GetTicketInfoAsync()
    {
        var request = new QqRequest
        {
            Path = "/getTicketInfo"
        };
        return transport.SendAsync(request);
    }
}
