using System.Text.Json;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Transport;

namespace QqMusic.Net.Protocol.Raw;

/// <summary>
///     评论域 Raw API：构建 <see cref="QqRequest"/> 并返回原始 <see cref="JsonElement"/>。
/// </summary>
public class RawCommentApi(IQqTransport transport)
{
    /// <summary>
    ///     GET /getComments — 评论列表。
    /// </summary>
    /// <param name="id">资源 ID（必填）。</param>
    /// <param name="pagesize">可选，每页条数。</param>
    /// <param name="pagenum">可选，页码。</param>
    /// <param name="cid">可选，评论楼层 ID。</param>
    /// <param name="cmd">可选，命令字。</param>
    /// <param name="reqtype">可选，请求类型。</param>
    /// <param name="biztype">可选，业务类型。</param>
    /// <param name="rootcommentid">可选，根评论 ID。</param>
    public Task<JsonElement> GetCommentsAsync(
        string id,
        int? pagesize = null,
        int? pagenum = null,
        int? cid = null,
        int? cmd = null,
        int? reqtype = null,
        int? biztype = null,
        string? rootcommentid = null)
    {
        var request = new QqRequest
        {
            Path = "/getComments",
            Params = new Dictionary<string, string>
            {
                ["id"] = id
            }
        };
        if (pagesize.HasValue)
            request.Params["pagesize"] = pagesize.Value.ToString();
        if (pagenum.HasValue)
            request.Params["pagenum"] = pagenum.Value.ToString();
        if (cid.HasValue)
            request.Params["cid"] = cid.Value.ToString();
        if (cmd.HasValue)
            request.Params["cmd"] = cmd.Value.ToString();
        if (reqtype.HasValue)
            request.Params["reqtype"] = reqtype.Value.ToString();
        if (biztype.HasValue)
            request.Params["biztype"] = biztype.Value.ToString();
        if (!string.IsNullOrWhiteSpace(rootcommentid))
            request.Params["rootcommentid"] = rootcommentid;
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getHotComments — 热门评论。
    /// </summary>
    /// <param name="id">资源 ID（必填）。</param>
    /// <param name="type">可选，类型：1 歌曲 / 2 歌单 / 3 专辑。</param>
    /// <param name="page">可选，页码。</param>
    /// <param name="pagesize">可选，每页条数。</param>
    public Task<JsonElement> GetHotCommentsAsync(
        string id,
        int? type = null,
        int? page = null,
        int? pagesize = null)
    {
        var request = new QqRequest
        {
            Path = "/getHotComments",
            Params = new Dictionary<string, string>
            {
                ["id"] = id
            }
        };
        if (type.HasValue)
            request.Params["type"] = type.Value.ToString();
        if (page.HasValue)
            request.Params["page"] = page.Value.ToString();
        if (pagesize.HasValue)
            request.Params["pagesize"] = pagesize.Value.ToString();
        return transport.SendAsync(request);
    }
}
