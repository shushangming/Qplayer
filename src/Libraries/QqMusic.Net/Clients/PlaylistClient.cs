using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Adapters.Common;
using QqMusic.Net.Protocol.Raw;
using QqMusic.Net.util;

namespace QqMusic.Net.Clients;

/// <summary>
///     歌单域客户端：调用 <see cref="RawPlaylistApi" /> 并用 <see cref="QqApiResponseParser" /> 反序列化为强类型模型。
///     每个方法都有对应的 Raw 版本（位于 <see cref="RawPlaylistApi" />）直接返回 <see cref="JsonElement" />。
/// </summary>
public class PlaylistClient(RawPlaylistApi rawApi)
{
    /// <summary>
    ///     GET /getSongListCategories —— 歌单分类。
    /// </summary>
    public async Task<QqPlaylistCategory?> GetSongListCategoriesAsync()
    {
        var json = await rawApi.GetSongListCategoriesAsync().ConfigureAwait(false);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqPlaylistCategory);
    }

    /// <summary>
    ///     GET /getSongLists —— 歌单列表。
    /// </summary>
    public async Task<QqPlaylistList?> GetSongListsAsync(int? limit = null, int? page = null, int? sortId = null,
        int? categoryId = null)
    {
        var json = await rawApi.GetSongListsAsync(limit, page, sortId, categoryId).ConfigureAwait(false);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqPlaylistList);
    }

    /// <summary>
    ///     GET /getSongListDetail —— 歌单详情。
    ///     真实结构：response.{code, cdlist:[{disstid, dissname, songlist:[...], ...}]}（取 cdlist[0]）。
    ///     注意：歌单详情歌曲的字段与搜索结构不同（mid/name/title/singer[]/album{}/file{media_mid}），
    ///     解析后需手动映射到 <see cref="QqSongInfo"/>，否则歌单歌曲为空。
    /// </summary>
    public async Task<QqPlaylistDetail?> GetSongListDetailAsync(string disstid, string? cookie = null)
    {
        var json = await rawApi.GetSongListDetailAsync(disstid, cookie).ConfigureAwait(false);
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        // 兼容 cdlist[0] 包裹和直接返回两种结构
        JsonElement first = payload;
        if (payload.TryGetProperty("cdlist", out var cdlist) &&
            cdlist.ValueKind == JsonValueKind.Array &&
            cdlist.GetArrayLength() > 0)
        {
            first = cdlist[0];
        }

        var detail = first.Deserialize(AppJsonContext.Default.QqPlaylistDetail);
        if (detail == null) return null;

        // 歌单详情歌曲手动映射（mid → SongMid、name/title → SongName、singer[]、album{} 等）
        if (first.TryGetProperty("songlist", out var songList) &&
            songList.ValueKind == JsonValueKind.Array)
        {
            var songs = new List<QqSongInfo>(songList.GetArrayLength());
            foreach (var s in songList.EnumerateArray())
                songs.Add(MapPlaylistSong(s));
            detail.SongList = songs;
        }

        return detail;
    }

    private static QqSongInfo MapPlaylistSong(JsonElement s)
    {
        var song = new QqSongInfo
        {
            SongId = GetInt64(s, "id"),
            Interval = GetInt32(s, "interval") ?? 0,
            Vid = GetString(GetObject(s, "mv"), "vid")
        };
        song.SongMid = GetString(s, "mid") ?? GetString(GetObject(s, "file"), "media_mid") ?? "";
        song.SongName = GetString(s, "name") ?? GetString(s, "title") ?? "";
        song.MediaMid = GetString(GetObject(s, "file"), "media_mid");
        song.AlbumMid = GetString(GetObject(s, "album"), "mid");
        song.AlbumId = GetInt64(GetObject(s, "album"), "id");
        song.AlbumName = GetString(GetObject(s, "album"), "name") ?? GetString(GetObject(s, "album"), "title");

        if (s.TryGetProperty("singer", out var singers) && singers.ValueKind == JsonValueKind.Array)
        {
            song.Singers = singers.EnumerateArray()
                .Select(x => new QqSingerLite
                {
                    Id = GetInt64(x, "id"),
                    Mid = GetString(x, "mid") ?? "",
                    Name = GetString(x, "name") ?? GetString(x, "title") ?? ""
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) || !string.IsNullOrWhiteSpace(x.Mid))
                .ToList();
        }

        return song;
    }

    private static string? GetString(JsonElement e, string key)
        => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(key, out var v) &&
           v.ValueKind is JsonValueKind.String or JsonValueKind.Number
            ? v.ToString()
            : null;

    private static JsonElement GetObject(JsonElement e, string key)
        => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(key, out var v)
            ? v
            : default;

    private static long? GetInt64(JsonElement e, string key)
        => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(key, out var v) &&
           v.ValueKind is JsonValueKind.Number or JsonValueKind.String
            ? (long.TryParse(v.ToString(), out var l) ? l : null)
            : null;

    private static int? GetInt32(JsonElement e, string key)
    {
        var l = GetInt64(e, key);
        return l.HasValue ? (int?)l.Value : null;
    }

    /// <summary>
    ///     POST /batchGetSongLists —— 批量获取歌单。
    /// </summary>
    public async Task<List<QqPlaylistDetail>> BatchGetSongListsAsync(QqBatchPlaylistRequest body)
    {
        var json = await rawApi.BatchGetSongListsAsync(body).ConfigureAwait(false);
        // payload 为歌单详情数组；List<QqPlaylistDetail> 未注册到源生成器，逐元素用已注册的类型反序列化。
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var result = new List<QqPlaylistDetail>();
        if (payload.ValueKind == JsonValueKind.Array)
            foreach (var item in payload.EnumerateArray())
            {
                var detail = item.Deserialize(AppJsonContext.Default.QqPlaylistDetail);
                if (detail is not null) result.Add(detail);
            }

        return result;
    }

    /// <summary>
    ///     GET /getPlaylistTags —— 歌单标签。
    /// </summary>
    public async Task<QqPlaylistTagList?> GetPlaylistTagsAsync()
    {
        var json = await rawApi.GetPlaylistTagsAsync().ConfigureAwait(false);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqPlaylistTagList);
    }

    /// <summary>
    ///     GET /getPlaylistsByTag —— 按标签获取歌单。
    /// </summary>
    public async Task<QqPlaylistByTag?> GetPlaylistsByTagAsync(string? tagId = null, int? page = null,
        int? num = null)
    {
        var json = await rawApi.GetPlaylistsByTagAsync(tagId, page, num).ConfigureAwait(false);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqPlaylistByTag);
    }

    /// <summary>
    ///     GET /resolveSongListShareUrl —— 解析歌单分享链接。
    /// </summary>
    public async Task<QqPlaylistShareResolve?> ResolveSongListShareUrlAsync(string url)
    {
        var json = await rawApi.ResolveSongListShareUrlAsync(url).ConfigureAwait(false);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqPlaylistShareResolve);
    }
}
