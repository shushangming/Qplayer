using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Transport;
using QqMusic.Net.util;

namespace QqMusic.Net.Protocol.Raw;

/// <summary>
///     登录域原始 API。BaseUrl 默认 localhost:3200，无签名。
/// </summary>
public class RawLoginApi(IQqTransport transport)
{
    /// <summary>
    ///     GET /getQQLoginQr — 创建登录二维码。
    /// </summary>
    public Task<JsonElement> GetQQLoginQrAsync()
    {
        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getQQLoginQr"
        });
    }

    /// <summary>
    ///     POST /checkQQLoginQr — 检查扫码状态。
    /// </summary>
    public Task<JsonElement> CheckQQLoginQrAsync(string qrsig, long ptqrtoken)
    {
        return transport.SendAsync(new QqRequest
        {
            Method = HttpMethod.Post,
            Path = "/checkQQLoginQr",
            Body = new QqLoginQrCheckRequest
            {
                QrSig = qrsig,
                PtQrToken = ptqrtoken
            },
            BodyTypeInfo = AppJsonContext.Default.QqLoginQrCheckRequest
        });
    }
}
