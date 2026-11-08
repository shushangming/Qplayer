using KgWebApi.Net.Extensions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers.Qq;

/// <summary>
///     QQ 音乐推荐相关API接口
/// </summary>
[ApiController]
[Route("qq/recommend")]
public class QqRecommendController(RecommendClient recommendClient, ILogger<QqRecommendController> logger) : ControllerBase
{
    /// <summary>
    ///     每日推荐。
    /// </summary>
    /// <param name="cookie">登录 cookie。</param>
    [HttpGet("daily")]
    public async Task<IActionResult> Daily([FromQuery] string? cookie = null)
    {
        try
        {
            var result = await recommendClient.GetDailyRecommendAsync(cookie);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取每日推荐异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     私人 FM。
    /// </summary>
    /// <param name="cookie">登录 cookie。</param>
    [HttpGet("fm")]
    public async Task<IActionResult> Fm([FromQuery] string? cookie = null)
    {
        try
        {
            var result = await recommendClient.GetPrivateFmAsync(cookie);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取私人FM异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     新歌速递。
    /// </summary>
    /// <param name="areaId">地区 id。</param>
    /// <param name="limit">数量。</param>
    [HttpGet("new")]
    public async Task<IActionResult> New([FromQuery] int? areaId = null, [FromQuery] int? limit = null)
    {
        try
        {
            var result = await recommendClient.GetNewSongsAsync(areaId, limit);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取新歌速递异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     首页推荐包。
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var result = await recommendClient.GetRecommendAsync();
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取首页推荐异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     推荐 Banner。
    /// </summary>
    [HttpGet("banner")]
    public async Task<IActionResult> Banner()
    {
        try
        {
            var result = await recommendClient.GetRecommendBannerAsync();
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取推荐Banner异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     个性化推荐。
    /// </summary>
    /// <param name="type">推荐类型。</param>
    /// <param name="cookie">登录 cookie。</param>
    [HttpGet("personal")]
    public async Task<IActionResult> Personal([FromQuery] string? type = null, [FromQuery] string? cookie = null)
    {
        try
        {
            var result = await recommendClient.GetPersonalRecommendAsync(type, cookie);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取个性化推荐异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     相似歌曲。
    /// </summary>
    /// <param name="songmid">歌曲 mid。</param>
    /// <param name="cookie">登录 cookie。</param>
    [HttpGet("similar")]
    public async Task<IActionResult> Similar(
        [FromQuery][Required(AllowEmptyStrings = false)] string songmid = "",
        [FromQuery] string? cookie = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(songmid)) return this.ApiBadRequest("songmid 不能为空", 40009);
            var result = await recommendClient.GetSimilarSongsAsync(songmid, cookie);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取相似歌曲异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     相关歌单。
    /// </summary>
    /// <param name="songid">歌曲 id。</param>
    /// <param name="sin">sin。</param>
    /// <param name="lastId">最后一条 id。</param>
    /// <param name="songType">歌曲类型。</param>
    [HttpGet("related")]
    public async Task<IActionResult> Related(
        [FromQuery] long? songid = null,
        [FromQuery] string? sin = null,
        [FromQuery] string? lastId = null,
        [FromQuery] int? songType = null)
    {
        try
        {
            var result = await recommendClient.GetRelatedPlaylistsAsync(songid, sin, lastId, songType);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取相关歌单异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }
}
