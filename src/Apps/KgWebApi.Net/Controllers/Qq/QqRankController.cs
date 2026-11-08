using KgWebApi.Net.Extensions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers.Qq;

/// <summary>
///     QQ 音乐排行榜相关API接口
/// </summary>
[ApiController]
[Route("qq/rank")]
public class QqRankController(RankClient rankClient, ILogger<QqRankController> logger) : ControllerBase
{
    /// <summary>
    ///     排行榜列表（榜单元数据）。
    /// </summary>
    [HttpGet("lists")]
    public async Task<IActionResult> Lists()
    {
        try
        {
            var result = await rankClient.GetTopListsAsync();
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取排行榜列表异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     排行榜详情。
    /// </summary>
    /// <param name="topId">榜单 id。</param>
    /// <param name="page">页码。</param>
    /// <param name="limit">数量。</param>
    /// <param name="resolveMid">是否解析 mid。</param>
    [HttpGet("detail")]
    public async Task<IActionResult> Detail(
        [FromQuery] int? topId = null,
        [FromQuery] int? page = null,
        [FromQuery] int? limit = null,
        [FromQuery] int? resolveMid = null)
    {
        try
        {
            var result = await rankClient.GetRanksAsync(topId, page, limit, resolveMid);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取排行榜详情异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }
}
