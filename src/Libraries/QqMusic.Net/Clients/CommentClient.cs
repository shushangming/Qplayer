using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Adapters.Common;
using QqMusic.Net.Protocol.Raw;
using QqMusic.Net.util;

namespace QqMusic.Net.Clients;

/// <summary>
///     评论域 Client：调用 <see cref="RawCommentApi"/> 并把 <see cref="JsonElement"/> 解析为强类型模型。
/// </summary>
public class CommentClient(RawCommentApi rawApi)
{
    /// <summary>
    ///     评论列表。真实结构：response.{..., comment:{commentlist:[...], ...}}（无 data 封装）。
    /// </summary>
    public async Task<QqCommentList?> GetCommentsAsync(
        string id,
        int? pagesize = null,
        int? pagenum = null,
        int? cid = null,
        int? cmd = null,
        int? reqtype = null,
        int? biztype = null,
        string? rootcommentid = null)
    {
        var json = await rawApi.GetCommentsAsync(id, pagesize, pagenum, cid, cmd, reqtype, biztype, rootcommentid);
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var comment = QqApiResponseParser.Drill(payload, "comment");
        return comment.Deserialize(AppJsonContext.Default.QqCommentList);
    }

    /// <summary>
    ///     评论列表（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetCommentsRawAsync(
        string id,
        int? pagesize = null,
        int? pagenum = null,
        int? cid = null,
        int? cmd = null,
        int? reqtype = null,
        int? biztype = null,
        string? rootcommentid = null)
    {
        return rawApi.GetCommentsAsync(id, pagesize, pagenum, cid, cmd, reqtype, biztype, rootcommentid);
    }

    /// <summary>
    ///     热门评论（type：1 歌曲 / 2 歌单 / 3 专辑）。真实结构：response.comment.{commentlist} 或错误码。
    /// </summary>
    public async Task<QqHotCommentList?> GetHotCommentsAsync(
        string id,
        int? type = null,
        int? page = null,
        int? pagesize = null)
    {
        var json = await rawApi.GetHotCommentsAsync(id, type, page, pagesize);
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var comment = QqApiResponseParser.Drill(payload, "comment");
        return comment.Deserialize(AppJsonContext.Default.QqHotCommentList);
    }

    /// <summary>
    ///     热门评论（原始 JSON）。
    /// </summary>
    public Task<JsonElement> GetHotCommentsRawAsync(
        string id,
        int? type = null,
        int? page = null,
        int? pagesize = null)
    {
        return rawApi.GetHotCommentsAsync(id, type, page, pagesize);
    }
}
