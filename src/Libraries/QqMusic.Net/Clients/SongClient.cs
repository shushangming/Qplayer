using System.Text.Json;
using QqMusic.Net.Abstractions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Adapters.Common;
using QqMusic.Net.Protocol.Raw;
using QqMusic.Net.util;

namespace QqMusic.Net.Clients;

/// <summary>
///     歌曲域 Client 层：基于 RawSongApi，把 JsonElement 解析为强类型模型。
/// </summary>
public class SongClient(RawSongApi rawApi)
{
    /// <summary>
    ///     获取播放地址（需登录 cookie）。
    /// </summary>
    public async Task<QqSongPlayResult?> GetPlayUrlAsync(string songmid,
        string quality = QqAudioQuality.Default, string? cookie = null)
    {
        var json = await rawApi.GetMusicPlayAsync(songmid, quality, cookie: cookie);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqSongPlayResult);
    }

    /// <summary>
    ///     获取播放地址，返回原始 JsonElement（不解析）。
    /// </summary>
    public async Task<JsonElement> GetPlayUrlRawAsync(string songmid,
        string quality = QqAudioQuality.Default, string? cookie = null)
    {
        return await rawApi.GetMusicPlayAsync(songmid, quality, cookie: cookie);
    }

    /// <summary>
    ///     获取歌曲信息（songmid 与 songid 二选一传入）。
    /// </summary>
    public async Task<QqSongInfo?> GetSongInfoAsync(string? songmid = null, long? songid = null)
    {
        var json = await rawApi.GetSongInfoAsync(songmid, songid);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqSongInfo);
    }

    /// <summary>
    ///     获取歌曲信息，返回原始 JsonElement（不解析）。
    /// </summary>
    public async Task<JsonElement> GetSongInfoRawAsync(string? songmid = null, long? songid = null)
    {
        return await rawApi.GetSongInfoAsync(songmid, songid);
    }

    /// <summary>
    ///     批量获取歌曲信息。
    /// </summary>
    public async Task<List<QqSongInfo>?> BatchGetSongInfoAsync(QqBatchSongInfoRequest request)
    {
        var json = await rawApi.BatchGetSongInfoAsync(request);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.ListQqSongInfo);
    }

    /// <summary>
    ///     批量获取歌曲信息，返回原始 JsonElement（不解析）。
    /// </summary>
    public async Task<JsonElement> BatchGetSongInfoRawAsync(QqBatchSongInfoRequest request)
    {
        return await rawApi.BatchGetSongInfoAsync(request);
    }
}
