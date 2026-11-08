using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Transport;
using QqMusic.Net.util;

namespace QqMusic.Net.Protocol.Raw;

/// <summary>
///     歌单域 Raw API：直接构建 <see cref="QqRequest" /> 并返回原始 <see cref="JsonElement" />。
///     GET 使用 Params，POST 使用 Body + BodyTypeInfo，无签名，BaseUrl 使用默认值。
/// </summary>
public class RawPlaylistApi(IQqTransport transport)
{
    /// <summary>
    ///     GET /getSongListCategories —— 歌单分类。
    /// </summary>
    public Task<JsonElement> GetSongListCategoriesAsync()
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSongListCategories"
        };
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getSongLists —— 歌单列表。
    /// </summary>
    public Task<JsonElement> GetSongListsAsync(int? limit = null, int? page = null, int? sortId = null,
        int? categoryId = null)
    {
        var query = new Dictionary<string, string>();
        if (limit.HasValue) query["limit"] = limit.Value.ToString();
        if (page.HasValue) query["page"] = page.Value.ToString();
        if (sortId.HasValue) query["sortId"] = sortId.Value.ToString();
        if (categoryId.HasValue) query["categoryId"] = categoryId.Value.ToString();

        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSongLists",
            Params = query
        };
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getSongListDetail —— 歌单详情。
    ///     <paramref name="cookie" /> 可选：拉取需登录的歌单（如"我喜欢的音乐"）时必须传入。
    /// </summary>
    public Task<JsonElement> GetSongListDetailAsync(string disstid, string? cookie = null)
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSongListDetail",
            Params = new Dictionary<string, string> { ["disstid"] = disstid }
        };
        if (!string.IsNullOrWhiteSpace(cookie))
            request.Params["cookie"] = cookie;
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     POST /batchGetSongLists —— 批量获取歌单。
    /// </summary>
    public Task<JsonElement> BatchGetSongListsAsync(QqBatchPlaylistRequest body)
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Post,
            Path = "/batchGetSongLists",
            Body = body,
            BodyTypeInfo = AppJsonContext.Default.QqBatchPlaylistRequest
        };
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getPlaylistTags —— 歌单标签。
    /// </summary>
    public Task<JsonElement> GetPlaylistTagsAsync()
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getPlaylistTags"
        };
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /getPlaylistsByTag —— 按标签获取歌单。
    /// </summary>
    public Task<JsonElement> GetPlaylistsByTagAsync(string? tagId = null, int? page = null, int? num = null)
    {
        var query = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(tagId)) query["tagId"] = tagId;
        if (page.HasValue) query["page"] = page.Value.ToString();
        if (num.HasValue) query["num"] = num.Value.ToString();

        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getPlaylistsByTag",
            Params = query
        };
        return transport.SendAsync(request);
    }

    /// <summary>
    ///     GET /resolveSongListShareUrl —— 解析歌单分享链接。
    /// </summary>
    public Task<JsonElement> ResolveSongListShareUrlAsync(string url)
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/resolveSongListShareUrl",
            Params = new Dictionary<string, string> { ["url"] = url }
        };
        return transport.SendAsync(request);
    }
}
