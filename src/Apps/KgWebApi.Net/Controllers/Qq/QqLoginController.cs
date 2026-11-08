using KgWebApi.Net.Extensions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers.Qq;

/// <summary>
///     QQ 音乐登录相关API接口
/// </summary>
[ApiController]
[Route("qq/login")]
public class QqLoginController(LoginClient loginClient, ILogger<QqLoginController> logger) : ControllerBase
{
    /// <summary>
    ///     创建登录二维码。
    /// </summary>
    [HttpGet("qr")]
    public async Task<IActionResult> Qr()
    {
        try
        {
            var result = await loginClient.GetQQLoginQrAsync();
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取登录二维码异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     检查扫码状态。
    /// </summary>
    /// <param name="request">扫码状态检查请求体。</param>
    [HttpPost("check")]
    public async Task<IActionResult> Check([FromBody] QqLoginCheckRequest request)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(request);
            if (string.IsNullOrWhiteSpace(request.Qrsig) || string.IsNullOrWhiteSpace(request.Ptqrtoken))
                return this.ApiBadRequest("qrsig 与 ptqrtoken 不能为空", 40009);
            if (!long.TryParse(request.Ptqrtoken, out var ptqrtoken))
                return this.ApiBadRequest("ptqrtoken 必须为数字", 40009);
            var result = await loginClient.CheckQQLoginQrAsync(request.Qrsig, ptqrtoken);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ检查扫码状态异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }
}

/// <summary>
///     检查扫码状态请求体。
/// </summary>
public sealed class QqLoginCheckRequest
{
    public string Qrsig { get; set; } = "";

    public string Ptqrtoken { get; set; } = "";
}
