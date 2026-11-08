using System.Text.Json;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Transport;

namespace QqMusic.Net.Protocol.Raw;

/// <summary>
///     歌词域 Raw API 层：直接构造 QqRequest 并通过 IQqTransport 发送，返回原始 JsonElement。
/// </summary>
public class RawLyricApi(IQqTransport transport)
{
    /// <summary>
    ///     获取歌词。
    ///     GET /getLyric?songmid&amp;isFormat&amp;cookie
    /// </summary>
    public async Task<JsonElement> GetLyricAsync(string songmid, int isFormat = 0, string? cookie = null)
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getLyric",
            Params = new Dictionary<string, string>
            {
                ["songmid"] = songmid,
                ["isFormat"] = isFormat.ToString()
            }
        };

        if (!string.IsNullOrEmpty(cookie))
            request.Params["cookie"] = cookie;

        return await transport.SendAsync(request);
    }
}
