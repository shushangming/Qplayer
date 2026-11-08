using KgWebApi.Net.Extensions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers.Qq;

/// <summary>
///     QQ 音乐歌曲相关API接口
/// </summary>
[ApiController]
[Route("qq/song")]
public class QqSongController(SongClient songClient, ILogger<QqSongController> logger) : ControllerBase
{
    /// <summary>
    ///     获取歌曲播放地址（需登录 cookie）。
    /// </summary>
    /// <param name="songmid">歌曲 mid。</param>
    /// <param name="quality">音质，默认 128。</param>
    /// <param name="cookie">登录 cookie。</param>
    [HttpGet("url")]
    public async Task<IActionResult> GetUrl(
        [FromQuery][Required(AllowEmptyStrings = false)] string songmid = "",
        [FromQuery] string quality = "128",
        [FromQuery] string? cookie = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(songmid)) return this.ApiBadRequest("songmid 不能为空", 40009);
            var result = await songClient.GetPlayUrlAsync(songmid, quality, cookie);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取播放地址异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     获取歌曲信息（songmid 与 songid 二选一传入）。
    /// </summary>
    /// <param name="songmid">歌曲 mid。</param>
    /// <param name="songid">歌曲 id。</param>
    [HttpGet("info")]
    public async Task<IActionResult> Info(
        [FromQuery] string? songmid = null,
        [FromQuery] long? songid = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(songmid) && songid is null)
                return this.ApiBadRequest("songmid 与 songid 不能同时为空", 40009);
            var result = await songClient.GetSongInfoAsync(songmid, songid);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取歌曲信息异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     批量获取歌曲信息。
    /// </summary>
    /// <param name="request">批量歌曲信息请求体。</param>
    [HttpPost("batch")]
    public async Task<IActionResult> BatchInfo([FromBody] QqBatchSongInfoRequest request)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(request);
            var result = await songClient.BatchGetSongInfoAsync(request);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ批量获取歌曲信息异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }
}
