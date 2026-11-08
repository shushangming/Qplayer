using KgWebApi.Net.Extensions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers.Qq;

/// <summary>
///     QQ 音乐歌手相关API接口
/// </summary>
[ApiController]
[Route("qq/artist")]
public class QqArtistController(ArtistClient artistClient, ILogger<QqArtistController> logger) : ControllerBase
{
    /// <summary>
    ///     歌手列表。
    /// </summary>
    /// <param name="area">地区，默认 -100 全部。</param>
    /// <param name="sex">性别，默认 -100 全部。</param>
    /// <param name="genre">流派，默认 -100 全部。</param>
    /// <param name="index">首字母索引（数字，-100=热门），默认 -100。</param>
    /// <param name="page">页码。</param>
    [HttpGet("list")]
    public async Task<IActionResult> List(
        [FromQuery] int area = -100,
        [FromQuery] int sex = -100,
        [FromQuery] int genre = -100,
        [FromQuery] int index = -100,
        [FromQuery] int page = 1)
    {
        try
        {
            var result = await artistClient.GetSingerListAsync(area, sex, genre, index, page);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取歌手列表异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     歌手分类。
    /// </summary>
    [HttpGet("category")]
    public async Task<IActionResult> Category()
    {
        try
        {
            var result = await artistClient.GetSingerCategoryAsync();
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取歌手分类异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     歌手简介。
    /// </summary>
    /// <param name="singermid">歌手 mid。</param>
    [HttpGet("desc")]
    public async Task<IActionResult> Desc(
        [FromQuery][Required(AllowEmptyStrings = false)] string singermid = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(singermid)) return this.ApiBadRequest("singermid 不能为空", 40009);
            var result = await artistClient.GetSingerDescAsync(singermid);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取歌手简介异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     歌手热门歌曲。
    /// </summary>
    /// <param name="singermid">歌手 mid。</param>
    /// <param name="limit">数量。</param>
    /// <param name="page">页码。</param>
    [HttpGet("hotsong")]
    public async Task<IActionResult> Hotsong(
        [FromQuery][Required(AllowEmptyStrings = false)] string singermid = "",
        [FromQuery] int limit = 30,
        [FromQuery] int page = 1)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(singermid)) return this.ApiBadRequest("singermid 不能为空", 40009);
            var result = await artistClient.GetSingerHotsongAsync(singermid, limit, page);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取歌手热门歌曲异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     歌手专辑。
    /// </summary>
    /// <param name="singermid">歌手 mid。</param>
    /// <param name="limit">数量。</param>
    /// <param name="page">页码。</param>
    [HttpGet("album")]
    public async Task<IActionResult> Album(
        [FromQuery][Required(AllowEmptyStrings = false)] string singermid = "",
        [FromQuery] int limit = 30,
        [FromQuery] int page = 1)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(singermid)) return this.ApiBadRequest("singermid 不能为空", 40009);
            var result = await artistClient.GetSingerAlbumAsync(singermid, limit, page);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取歌手专辑异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     歌手 MV。
    /// </summary>
    /// <param name="singermid">歌手 mid。</param>
    /// <param name="order">排序，默认 time。</param>
    /// <param name="num">数量。</param>
    [HttpGet("mv")]
    public async Task<IActionResult> Mv(
        [FromQuery][Required(AllowEmptyStrings = false)] string singermid = "",
        [FromQuery] string order = "time",
        [FromQuery] int num = 30)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(singermid)) return this.ApiBadRequest("singermid 不能为空", 40009);
            var result = await artistClient.GetSingerMvAsync(singermid, order, num);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取歌手MV异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     相似歌手。
    /// </summary>
    /// <param name="singermid">歌手 mid。</param>
    [HttpGet("similar")]
    public async Task<IActionResult> Similar(
        [FromQuery][Required(AllowEmptyStrings = false)] string singermid = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(singermid)) return this.ApiBadRequest("singermid 不能为空", 40009);
            var result = await artistClient.GetSimilarSingerAsync(singermid);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取相似歌手异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     歌手星数（人气）。
    /// </summary>
    /// <param name="singermid">歌手 mid。</param>
    [HttpGet("starnum")]
    public async Task<IActionResult> StarNum(
        [FromQuery][Required(AllowEmptyStrings = false)] string singermid = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(singermid)) return this.ApiBadRequest("singermid 不能为空", 40009);
            var result = await artistClient.GetSingerStarNumAsync(singermid);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取歌手星数异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }
}
