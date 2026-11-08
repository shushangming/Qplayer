using KgWebApi.Net.Extensions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers.Qq;

/// <summary>
///     QQ 音乐歌词相关API接口
/// </summary>
[ApiController]
[Route("qq/lyric")]
public class QqLyricController(LyricClient lyricClient, ILogger<QqLyricController> logger) : ControllerBase
{
    /// <summary>
    ///     获取歌词。
    /// </summary>
    /// <param name="songmid">歌曲 mid。</param>
    /// <param name="isFormat">是否格式化，默认 0。</param>
    /// <param name="cookie">登录 cookie。</param>
    [HttpGet]
    public async Task<IActionResult> Lyric(
        [FromQuery][Required(AllowEmptyStrings = false)] string songmid = "",
        [FromQuery] int isFormat = 0,
        [FromQuery] string? cookie = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(songmid)) return this.ApiBadRequest("songmid 不能为空", 40009);
            var result = await lyricClient.GetLyricAsync(songmid, isFormat, cookie);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取歌词异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }
}
