using KgWebApi.Net.Extensions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers.Qq;

/// <summary>
///     QQ 音乐搜索相关API接口
/// </summary>
[ApiController]
[Route("qq/search")]
public class QqSearchController(SearchClient searchClient, ILogger<QqSearchController> logger) : ControllerBase
{
    /// <summary>
    ///     综合搜索。
    /// </summary>
    /// <param name="key">关键词。</param>
    /// <param name="limit">返回数量。</param>
    /// <param name="page">页码。</param>
    /// <param name="remoteplace">搜索域，默认为 song。</param>
    /// <param name="catZhida">直达词分类。</param>
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery][Required(AllowEmptyStrings = false)] string key = "",
        [FromQuery] int limit = 20,
        [FromQuery] int page = 1,
        [FromQuery] string remoteplace = "song",
        [FromQuery] string? catZhida = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key)) return this.ApiBadRequest("关键词不能为空", 40009);
            var result = await searchClient.SearchAsync(key, limit, page, remoteplace, catZhida);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ搜索异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     搜索联想（智能盒子）。
    /// </summary>
    /// <param name="key">关键词。</param>
    [HttpGet("smartbox")]
    public async Task<IActionResult> Smartbox(
        [FromQuery][Required(AllowEmptyStrings = false)] string key = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key)) return this.ApiBadRequest("关键词不能为空", 40009);
            var result = await searchClient.GetSmartboxAsync(key);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ搜索联想异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     热搜列表。
    /// </summary>
    [HttpGet("hot")]
    public async Task<IActionResult> Hot()
    {
        try
        {
            var result = await searchClient.GetHotKeyAsync();
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ热搜异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }
}
