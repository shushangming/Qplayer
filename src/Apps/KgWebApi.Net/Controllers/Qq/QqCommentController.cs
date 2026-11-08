using KgWebApi.Net.Extensions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KgWebApi.Net.Controllers.Qq;

/// <summary>
///     QQ 音乐评论相关API接口
/// </summary>
[ApiController]
[Route("qq/comment")]
public class QqCommentController(CommentClient commentClient, ILogger<QqCommentController> logger) : ControllerBase
{
    /// <summary>
    ///     评论列表。
    /// </summary>
    /// <param name="id">资源 id。</param>
    /// <param name="pagesize">每页数量。</param>
    /// <param name="pagenum">页码。</param>
    /// <param name="cid">评论 id。</param>
    /// <param name="cmd">命令。</param>
    /// <param name="reqtype">请求类型。</param>
    /// <param name="biztype">业务类型。</param>
    /// <param name="rootcommentid">根评论 id。</param>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery][Required(AllowEmptyStrings = false)] string id = "",
        [FromQuery] int? pagesize = null,
        [FromQuery] int? pagenum = null,
        [FromQuery] int? cid = null,
        [FromQuery] int? cmd = null,
        [FromQuery] int? reqtype = null,
        [FromQuery] int? biztype = null,
        [FromQuery] string? rootcommentid = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id)) return this.ApiBadRequest("id 不能为空", 40009);
            var result = await commentClient.GetCommentsAsync(id, pagesize, pagenum, cid, cmd, reqtype, biztype, rootcommentid);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取评论列表异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }

    /// <summary>
    ///     热门评论（type：1 歌曲 / 2 歌单 / 3 专辑）。
    /// </summary>
    /// <param name="id">资源 id。</param>
    /// <param name="type">资源类型。</param>
    /// <param name="page">页码。</param>
    /// <param name="pagesize">每页数量。</param>
    [HttpGet("hot")]
    public async Task<IActionResult> Hot(
        [FromQuery][Required(AllowEmptyStrings = false)] string id = "",
        [FromQuery] int? type = null,
        [FromQuery] int? page = null,
        [FromQuery] int? pagesize = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id)) return this.ApiBadRequest("id 不能为空", 40009);
            var result = await commentClient.GetHotCommentsAsync(id, type, page, pagesize);
            return Ok(result);
        }
        catch (TaskCanceledException)
        {
            return this.ApiGatewayTimeout("请求超时", 50401);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "QQ获取热门评论异常");
            return this.ApiServerError($"内部服务器错误: {ex.Message}", 50002);
        }
    }
}
