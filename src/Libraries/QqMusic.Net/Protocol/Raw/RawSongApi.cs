using System.Text.Json;
using QqMusic.Net.Abstractions;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Transport;
using QqMusic.Net.util;

namespace QqMusic.Net.Protocol.Raw;

/// <summary>
///     歌曲域 Raw API 层：直接构造 QqRequest 并通过 IQqTransport 发送，返回原始 JsonElement。
/// </summary>
public class RawSongApi(IQqTransport transport)
{
    /// <summary>
    ///     获取播放地址（需登录 cookie，未登录返回错误）。
    ///     GET /getMusicPlay?songmid&amp;resType&amp;mediaId&amp;quality&amp;cookie
    ///     quality: 128/320/flac/m4a/ape，见 <see cref="QqAudioQuality"/>。
    /// </summary>
    public async Task<JsonElement> GetMusicPlayAsync(string songmid, string quality = QqAudioQuality.Default,
        string? resType = null, string? mediaId = null, string? cookie = null)
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getMusicPlay",
            Params = new Dictionary<string, string>
            {
                ["songmid"] = songmid,
                ["quality"] = quality
            }
        };

        if (!string.IsNullOrEmpty(resType))
            request.Params["resType"] = resType;
        if (!string.IsNullOrEmpty(mediaId))
            request.Params["mediaId"] = mediaId;
        if (!string.IsNullOrEmpty(cookie))
            request.Params["cookie"] = cookie;

        return await transport.SendAsync(request);
    }

    /// <summary>
    ///     获取歌曲信息（songmid 与 songid 二选一传入）。
    ///     GET /getSongInfo?songmid 或 ?songid
    /// </summary>
    public async Task<JsonElement> GetSongInfoAsync(string? songmid = null, long? songid = null)
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Get,
            Path = "/getSongInfo",
            Params = new Dictionary<string, string>()
        };

        if (!string.IsNullOrEmpty(songmid))
            request.Params["songmid"] = songmid;
        if (songid.HasValue)
            request.Params["songid"] = songid.Value.ToString();

        return await transport.SendAsync(request);
    }

    /// <summary>
    ///     批量获取歌曲信息。
    ///     POST /batchGetSongInfo，body 为 {songs:[["songmid","songid"]]}。
    /// </summary>
    public async Task<JsonElement> BatchGetSongInfoAsync(QqBatchSongInfoRequest body)
    {
        var request = new QqRequest
        {
            Method = HttpMethod.Post,
            Path = "/batchGetSongInfo",
            Body = body,
            BodyTypeInfo = AppJsonContext.Default.QqBatchSongInfoRequest
        };

        return await transport.SendAsync(request);
    }
}
