using KgWebApi.Net.Extensions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers.Qq;

/// <summary>
///     QQ 音乐图片/杂项相关API接口
/// </summary>
[ApiController]
[Route("qq/image")]
public class QqImageController(ImageClient imageClient, ILogger<QqImageController> logger) : ControllerBase
{
    /// <summary>
    ///     图片 URL（size：150x150 / 300x300 / 500x500 / 800x800）。
    /// </summary>
    /// <param name="id">图片 id。</param>
    /// <param name="size">尺寸。</param>
    /// <param name="maxAge">缓存最大时长。</param>
    [HttpGet("url")]
    public async Task<IActionResult> GetUrl(
        [FromQuery][Required(AllowEmptyStrings = false)] string id = "",
        [FromQuery] string? size = null,
        [FromQuery] int? maxAge = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id)) return this.ApiBadRequest("id 不能为空", 40009);
            var result = await imageClient.GetImageUrlAsync(id, size, maxAge);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取图片URL异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     下载 QQ 音乐客户端信息。
    /// </summary>
    [HttpGet("download")]
    public async Task<IActionResult> Download()
    {
        try
        {
            var result = await imageClient.GetDownloadQQMusicAsync();
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取下载客户端信息异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     票据信息。
    /// </summary>
    [HttpGet("ticket")]
    public async Task<IActionResult> Ticket()
    {
        try
        {
            var result = await imageClient.GetTicketInfoAsync();
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取票据信息异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }
}
