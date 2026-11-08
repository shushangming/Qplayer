using KgWebApi.Net.Extensions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers.Qq;

/// <summary>
///     QQ 音乐用户相关API接口
/// </summary>
[ApiController]
[Route("qq/user")]
public class QqUserController(UserClient userClient, ILogger<QqUserController> logger) : ControllerBase
{
    /// <summary>
    ///     用户歌单。
    /// </summary>
    /// <param name="uin">用户 uin。</param>
    /// <param name="offset">偏移。</param>
    /// <param name="limit">数量。</param>
    /// <param name="cookie">登录 cookie。</param>
    [HttpGet("playlists")]
    public async Task<IActionResult> Playlists(
        [FromQuery][Required] long uin,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 30,
        [FromQuery] string? cookie = null)
    {
        try
        {
            var result = await userClient.GetUserPlaylistsAsync(uin, offset, limit, cookie);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取用户歌单异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     用户详情。
    /// </summary>
    /// <param name="uin">用户 uin。</param>
    /// <param name="cookie">登录 cookie。</param>
    [HttpGet("detail")]
    public async Task<IActionResult> Detail(
        [FromQuery][Required] long uin,
        [FromQuery] string? cookie = null)
    {
        try
        {
            var result = await userClient.GetUserDetailAsync(uin, cookie);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取用户详情异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     用户收藏歌单。
    /// </summary>
    /// <param name="uin">用户 uin。</param>
    /// <param name="page">页码。</param>
    /// <param name="limit">数量。</param>
    /// <param name="cookie">登录 cookie。</param>
    [HttpGet("collected")]
    public async Task<IActionResult> Collected(
        [FromQuery][Required] long uin,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 30,
        [FromQuery] string? cookie = null)
    {
        try
        {
            var result = await userClient.GetUserCollectedSongListsAsync(uin, page, limit, cookie);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取用户收藏歌单异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     用户关注歌手。
    /// </summary>
    /// <param name="cookie">登录 cookie。</param>
    [HttpGet("follow-singers")]
    public async Task<IActionResult> FollowSingers([FromQuery] string? cookie = null)
    {
        try
        {
            var result = await userClient.GetUserFollowSingersAsync(cookie);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取用户关注歌手异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     用户头像（uin 或 k 二选一）。
    /// </summary>
    /// <param name="uin">用户 uin。</param>
    /// <param name="k">头像 key。</param>
    /// <param name="size">尺寸。</param>
    [HttpGet("avatar")]
    public async Task<IActionResult> Avatar(
        [FromQuery] long? uin = null,
        [FromQuery] string? k = null,
        [FromQuery] int size = 100)
    {
        try
        {
            if (uin is null && string.IsNullOrWhiteSpace(k))
                return this.ApiBadRequest("uin 与 k 不能同时为空", 40009);
            var result = await userClient.GetUserAvatarAsync(uin, k, size);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取用户头像异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     用户喜欢的音乐（需 cookie）。
    /// </summary>
    /// <param name="cookie">登录 cookie。</param>
    [HttpGet("liked")]
    public async Task<IActionResult> Liked([FromQuery] string? cookie = null)
    {
        try
        {
            var result = await userClient.GetUserLikedSongsAsync(cookie);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取用户喜欢的音乐异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     VIP 信息。
    /// </summary>
    /// <param name="cookie">登录 cookie。</param>
    [HttpGet("vip")]
    public async Task<IActionResult> Vip([FromQuery] string? cookie = null)
    {
        try
        {
            var result = await userClient.GetVipInfoAsync(cookie);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取VIP信息异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     获取当前 cookie。
    /// </summary>
    [HttpGet("cookie")]
    public async Task<IActionResult> Cookie()
    {
        try
        {
            var result = await userClient.GetCookieInfoAsync();
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取cookie异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     设置 cookie。
    /// </summary>
    /// <param name="cookie">要设置的 cookie。</param>
    [HttpPost("cookie")]
    public async Task<IActionResult> SetCookie([FromBody] QqSetCookieRequest cookie)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(cookie);
            if (string.IsNullOrWhiteSpace(cookie.Cookie))
                return this.ApiBadRequest("cookie 不能为空", 40009);
            var result = await userClient.SetCookieAsync(cookie.Cookie);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ设置cookie异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }
}

/// <summary>
///     设置 cookie 请求体。
/// </summary>
public sealed class QqSetCookieRequest
{
    public string Cookie { get; set; } = "";
}
