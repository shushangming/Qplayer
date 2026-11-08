using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Adapters.Common;
using QqMusic.Net.Protocol.Raw;
using QqMusic.Net.util;

namespace QqMusic.Net.Clients;

/// <summary>
///     专辑域客户端：调用 <see cref="RawAlbumApi" /> 并用 <see cref="QqApiResponseParser" /> 反序列化为强类型模型。
/// </summary>
public class AlbumClient(RawAlbumApi rawApi)
{
    /// <summary>
    ///     GET /getAlbumSongs —— 专辑歌曲列表。
    ///     真实结构：response.albumSonglist.data.{albumMid, totalNum, songList:[{songInfo:{...}}]}
    /// </summary>
    public async Task<QqAlbumSongs?> GetAlbumSongsAsync(string? albummid = null, long? albumid = null,
        int? begin = null, int? limit = null, int? order = null)
    {
        var json = await rawApi.GetAlbumSongsAsync(albummid, albumid, begin, limit, order).ConfigureAwait(false);
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var data = QqApiResponseParser.Drill(payload, "albumSonglist", "data");

        // 解出基础字段
        var result = data.Deserialize(AppJsonContext.Default.QqAlbumSongs);
        // songList 元素被 {songInfo:{...}} 包裹，需解包
        if (result != null && data.TryGetProperty("songList", out var songList) &&
            songList.ValueKind == JsonValueKind.Array)
        {
            var songs = new List<QqSongInfo>();
            foreach (var item in songList.EnumerateArray())
            {
                if (item.TryGetProperty("songInfo", out var songInfo) &&
                    songInfo.ValueKind == JsonValueKind.Object)
                {
                    var song = songInfo.Deserialize(AppJsonContext.Default.QqSongInfo);
                    if (song != null) songs.Add(song);
                }
            }
            result.Songs = songs;
        }
        return result;
    }

    /// <summary>
    ///     GET /getAlbumInfo —— 专辑信息。真实结构：response.data.{...}
    /// </summary>
    public async Task<QqAlbumInfo?> GetAlbumInfoAsync(string albummid)
    {
        var json = await rawApi.GetAlbumInfoAsync(albummid).ConfigureAwait(false);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqAlbumInfo);
    }

    /// <summary>
    ///     GET /getNewDisks —— 新碟上架。真实结构：response.new_album.data.{albums, total}
    /// </summary>
    public async Task<QqNewDisks?> GetNewDisksAsync(int? page = null, int? limit = null)
    {
        var json = await rawApi.GetNewDisksAsync(page, limit).ConfigureAwait(false);
        var payload = QqApiResponseParser.Parse(json, AppJsonContext.Default.JsonElement);
        var newAlbumData = QqApiResponseParser.Drill(payload, "new_album", "data");
        return newAlbumData.Deserialize(AppJsonContext.Default.QqNewDisks);
    }

    /// <summary>
    ///     GET /getAlbumSongs —— 原始 JsonElement。
    /// </summary>
    public Task<JsonElement> GetAlbumSongsRawAsync(string? albummid = null, long? albumid = null,
        int? begin = null, int? limit = null, int? order = null)
    {
        return rawApi.GetAlbumSongsAsync(albummid, albumid, begin, limit, order);
    }

    /// <summary>
    ///     GET /getAlbumInfo —— 原始 JsonElement。
    /// </summary>
    public Task<JsonElement> GetAlbumInfoRawAsync(string albummid)
    {
        return rawApi.GetAlbumInfoAsync(albummid);
    }

    /// <summary>
    ///     GET /getNewDisks —— 原始 JsonElement。
    /// </summary>
    public Task<JsonElement> GetNewDisksRawAsync(int? page = null, int? limit = null)
    {
        return rawApi.GetNewDisksAsync(page, limit);
    }
}
