using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Adapters.Common;
using QqMusic.Net.Protocol.Raw;
using QqMusic.Net.util;

namespace QqMusic.Net.Clients;

/// <summary>
///     登录域客户端。在 Raw API 之上做强类型反序列化。
/// </summary>
public class LoginClient(RawLoginApi rawApi)
{
    /// <summary>
    ///     创建登录二维码。
    /// </summary>
    public async Task<QqLoginQr?> GetQQLoginQrAsync()
    {
        var json = await rawApi.GetQQLoginQrAsync();
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqLoginQr);
    }

    /// <summary>
    ///     创建登录二维码（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> GetQQLoginQrRawAsync()
    {
        return rawApi.GetQQLoginQrAsync();
    }

    /// <summary>
    ///     检查扫码状态。
    /// </summary>
    public async Task<QqLoginQrResult?> CheckQQLoginQrAsync(string qrsig, long ptqrtoken)
    {
        var json = await rawApi.CheckQQLoginQrAsync(qrsig, ptqrtoken);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqLoginQrResult);
    }

    /// <summary>
    ///     检查扫码状态（原始 JsonElement）。
    /// </summary>
    public Task<JsonElement> CheckQQLoginQrRawAsync(string qrsig, long ptqrtoken)
    {
        return rawApi.CheckQQLoginQrAsync(qrsig, ptqrtoken);
    }
}
