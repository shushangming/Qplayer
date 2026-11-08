using System.Text.Json.Serialization;

namespace QqMusic.Net.Abstractions.Models;

/// <summary>
///     登录二维码（GET /getQQLoginQr）。
///     真实响应 payload：{ img, qrsig, ptqrtoken }。
/// </summary>
public record QqLoginQr : QqBaseModel
{
    /// <summary>
    ///     二维码图片（base64 data URI 或图片地址）。
    /// </summary>
    [property: JsonPropertyName("img")]
    public string? Img { get; set; }

    [property: JsonPropertyName("qrsig")]
    public string? QrSig { get; set; }

    [property: JsonPropertyName("ptqrtoken")]
    public long? PtQrToken { get; set; }
}

/// <summary>
///     扫码登录检查请求体。
/// </summary>
public record QqLoginQrCheckRequest
{
    [property: JsonPropertyName("qrsig")]
    public string QrSig { get; set; } = "";

    [property: JsonPropertyName("ptqrtoken")]
    public long PtQrToken { get; set; }
}

/// <summary>
///     扫码登录状态结果（POST /checkQQLoginQr）。
///     真实响应 payload：
///     - 未扫码/已扫码未确认：{ isOk: false, refresh: false, message: "未扫描二维码" }
///     - 二维码失效：{ isOk: false, refresh: true, message: "二维码已失效" }
///     - 登录成功：{ isOk: true, message: "登录成功", session: { loginUin, uin, cookie, cookieList, cookieObject } }
///     外层 code/message 由 <see cref="QqBaseModel"/> 捕获。
/// </summary>
public record QqLoginQrResult : QqBaseModel
{
    /// <summary>
    ///     是否登录成功。
    /// </summary>
    [property: JsonPropertyName("isOk")]
    public bool? IsOk { get; set; }

    /// <summary>
    ///     二维码是否已失效（需要刷新）。
    /// </summary>
    [property: JsonPropertyName("refresh")]
    public bool? Refresh { get; set; }

    /// <summary>
    ///     登录成功后返回的会话信息（含完整 cookie 字符串）。
    /// </summary>
    [property: JsonPropertyName("session")]
    public QqLoginSession? Session { get; set; }

    /// <summary>
    ///     是否等待扫码。
    /// </summary>
    [JsonIgnore]
    public bool IsWaiting => IsOk != true && Refresh != true;

    /// <summary>
    ///     二维码是否已过期。
    /// </summary>
    [JsonIgnore]
    public bool IsExpired => Refresh == true;

    /// <summary>
    ///     是否已确认登录成功。
    /// </summary>
    [JsonIgnore]
    public bool IsConfirmed => IsOk == true;
}

/// <summary>
///     QQ 音乐登录会话（checkQQLoginQr 成功时的 session 对象）。
/// </summary>
public record QqLoginSession
{
    /// <summary>
    ///     登录 uin（cookie 中解析）。
    /// </summary>
    [property: JsonPropertyName("loginUin")]
    public string? LoginUin { get; set; }

    [property: JsonPropertyName("uin")]
    public string? Uin { get; set; }

    /// <summary>
    ///     完整 cookie 字符串（用于后续带登录态调用）。
    /// </summary>
    [property: JsonPropertyName("cookie")]
    public string? Cookie { get; set; }

    [property: JsonPropertyName("cookieList")]
    public List<string>? CookieList { get; set; }

    [property: JsonPropertyName("cookieObject")]
    public Dictionary<string, string>? CookieObject { get; set; }
}
