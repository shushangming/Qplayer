using KgWebApi.Net.Extensions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers.Qq;

/// <summary>
///     QQ 音乐专辑相关API接口
/// </summary>
[ApiController]
[Route("qq/album")]
public class QqAlbumController(AlbumClient albumClient, ILogger<QqAlbumController> logger) : ControllerBase
{
    /// <summary>
    ///     专辑歌曲列表（albummid 与 albumid 二选一）。
    /// </summary>
    /// <param name="albummid">专辑 mid。</param>
    /// <param name="albumid">专辑 id。</param>
    /// <param name="begin">起始位置。</param>
    /// <param name="limit">数量。</param>
    /// <param name="order">排序。</param>
    [HttpGet("songs")]
    public async Task<IActionResult> Songs(
        [FromQuery] string? albummid = null,
        [FromQuery] long? albumid = null,
        [FromQuery] int? begin = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? order = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(albummid) && albumid is null)
                return this.ApiBadRequest("albummid 与 albumid 不能同时为空", 40009);
            var result = await albumClient.GetAlbumSongsAsync(albummid, albumid, begin, limit, order);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取专辑歌曲异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     专辑信息。
    /// </summary>
    /// <param name="albummid">专辑 mid。</param>
    [HttpGet("info")]
    public async Task<IActionResult> Info(
        [FromQuery][Required(AllowEmptyStrings = false)] string albummid = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(albummid)) return this.ApiBadRequest("albummid 不能为空", 40009);
            var result = await albumClient.GetAlbumInfoAsync(albummid);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取专辑信息异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     新碟上架。
    /// </summary>
    /// <param name="page">页码。</param>
    /// <param name="limit">数量。</param>
    [HttpGet("new")]
    public async Task<IActionResult> New(
        [FromQuery] int? page = null,
        [FromQuery] int? limit = null)
    {
        try
        {
            var result = await albumClient.GetNewDisksAsync(page, limit);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取新碟异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }
}
