using KgWebApi.Net.Extensions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers.Qq;

/// <summary>
///     QQ 音乐 MV 相关API接口
/// </summary>
[ApiController]
[Route("qq/mv")]
public class QqMvController(MvClient mvClient, ILogger<QqMvController> logger) : ControllerBase
{
    /// <summary>
    ///     MV 播放地址。
    /// </summary>
    /// <param name="vid">MV vid。</param>
    [HttpGet("play")]
    public async Task<IActionResult> Play(
        [FromQuery][Required(AllowEmptyStrings = false)] string vid = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(vid)) return this.ApiBadRequest("vid 不能为空", 40009);
            var result = await mvClient.GetMvPlayAsync(vid);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取MV播放地址异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     MV 列表。
    /// </summary>
    /// <param name="area_id">地区 id。</param>
    /// <param name="version_id">版本 id。</param>
    /// <param name="limit">数量。</param>
    /// <param name="page">页码。</param>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int? area_id = null,
        [FromQuery] int? version_id = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? page = null)
    {
        try
        {
            var result = await mvClient.GetMvAsync(area_id, version_id, limit, page);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取MV列表异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     MV 分类。
    /// </summary>
    [HttpGet("category")]
    public async Task<IActionResult> Category()
    {
        try
        {
            var result = await mvClient.GetMvCategoryAsync();
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取MV分类异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     按标签获取 MV。
    /// </summary>
    /// <param name="tag">标签。</param>
    [HttpGet("byTag")]
    public async Task<IActionResult> ByTag([FromQuery] string? tag = null)
    {
        try
        {
            var result = await mvClient.GetMvByTagAsync(tag);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ按标签获取MV异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }
}
