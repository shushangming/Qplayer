using KgWebApi.Net.Extensions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers.Qq;

/// <summary>
///     QQ 音乐歌单相关API接口
/// </summary>
[ApiController]
[Route("qq/playlist")]
public class QqPlaylistController(PlaylistClient playlistClient, ILogger<QqPlaylistController> logger) : ControllerBase
{
    /// <summary>
    ///     歌单分类。
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        try
        {
            var result = await playlistClient.GetSongListCategoriesAsync();
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取歌单分类异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     歌单列表。
    /// </summary>
    /// <param name="limit">返回数量。</param>
    /// <param name="page">页码。</param>
    /// <param name="sortId">排序 id。</param>
    /// <param name="categoryId">分类 id。</param>
    [HttpGet]
    public async Task<IActionResult> Lists(
        [FromQuery] int? limit = null,
        [FromQuery] int? page = null,
        [FromQuery] int? sortId = null,
        [FromQuery] int? categoryId = null)
    {
        try
        {
            var result = await playlistClient.GetSongListsAsync(limit, page, sortId, categoryId);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取歌单列表异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     歌单详情。
    /// </summary>
    /// <param name="disstid">歌单 id。</param>
    [HttpGet("detail")]
    public async Task<IActionResult> Detail(
        [FromQuery][Required(AllowEmptyStrings = false)] string disstid = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(disstid)) return this.ApiBadRequest("disstid 不能为空", 40009);
            var result = await playlistClient.GetSongListDetailAsync(disstid);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取歌单详情异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     批量获取歌单。
    /// </summary>
    /// <param name="body">批量歌单请求体。</param>
    [HttpPost("batch")]
    public async Task<IActionResult> Batch([FromBody] QqBatchPlaylistRequest body)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(body);
            var result = await playlistClient.BatchGetSongListsAsync(body);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ批量获取歌单异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     歌单标签。
    /// </summary>
    [HttpGet("tags")]
    public async Task<IActionResult> Tags()
    {
        try
        {
            var result = await playlistClient.GetPlaylistTagsAsync();
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取歌单标签异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     按标签获取歌单。
    /// </summary>
    /// <param name="tagId">标签 id。</param>
    /// <param name="page">页码。</param>
    /// <param name="num">数量。</param>
    [HttpGet("byTag")]
    public async Task<IActionResult> ByTag(
        [FromQuery] string? tagId = null,
        [FromQuery] int? page = null,
        [FromQuery] int? num = null)
    {
        try
        {
            var result = await playlistClient.GetPlaylistsByTagAsync(tagId, page, num);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ按标签获取歌单异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     解析歌单分享链接。
    /// </summary>
    /// <param name="url">歌单分享链接。</param>
    [HttpGet("resolve")]
    public async Task<IActionResult> Resolve(
        [FromQuery][Required(AllowEmptyStrings = false)] string url = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url)) return this.ApiBadRequest("url 不能为空", 40009);
            var result = await playlistClient.ResolveSongListShareUrlAsync(url);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ解析歌单分享链接异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }
}
