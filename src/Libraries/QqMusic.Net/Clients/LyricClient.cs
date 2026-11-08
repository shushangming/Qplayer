using System.Text.Json;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Adapters.Common;
using QqMusic.Net.Protocol.Raw;
using QqMusic.Net.util;

namespace QqMusic.Net.Clients;

/// <summary>
///     歌词域 Client 层：基于 RawLyricApi，把 JsonElement 解析为强类型模型。
/// </summary>
public class LyricClient(RawLyricApi rawApi)
{
    /// <summary>
    ///     获取歌词。
    /// </summary>
    public async Task<QqLyricResult?> GetLyricAsync(string songmid, int isFormat = 0, string? cookie = null)
    {
        var json = await rawApi.GetLyricAsync(songmid, isFormat, cookie);
        return QqApiResponseParser.Parse(json, AppJsonContext.Default.QqLyricResult);
    }

    /// <summary>
    ///     获取歌词，返回原始 JsonElement（不解析）。
    /// </summary>
    public async Task<JsonElement> GetLyricRawAsync(string songmid, int isFormat = 0, string? cookie = null)
    {
        return await rawApi.GetLyricAsync(songmid, isFormat, cookie);
    }
}
