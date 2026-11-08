using System;
using System.Collections.Generic;
using System.IO;
using ZLinq;
using System.Threading;
using System.Threading.Tasks;
using KuGou.Net.Abstractions;
using KuGou.Net.Abstractions.Models;
using KuGou.Net.Clients;
using KuGou.Net.Protocol.Session;
using KugouAvaloniaPlayer.Services.Jellyfin;
using KugouAvaloniaPlayer.Models;
using KugouAvaloniaPlayer.ViewModels;
using Microsoft.Extensions.Logging;
using QqSongClient = QqMusic.Net.Clients.SongClient;

namespace KugouAvaloniaPlayer.Services;

public interface IPlaybackSourceResolver
{
    Task<PlaybackSourceResult> ResolveAsync(SongItem song, string quality, CancellationToken cancellationToken);
}

public sealed class PlaybackSourceResolver(
    SongClient songClient,
    UserClient userClient,
    KgSessionManager sessionManager,
    IJellyfinClient jellyfinClient,
    QqSongClient qqSongClient,
    QqCookieService qqCookieService,
    ILogger<PlaybackSourceResolver> logger)
    : IPlaybackSourceResolver
{
    public async Task<PlaybackSourceResult> ResolveAsync(
        SongItem song,
        string quality,
        CancellationToken cancellationToken)
    {
        var localFilePath = song.LocalFilePath;
        if (string.Equals(song.LocalSourceType, LocalMusicLibraryService.SourceTypeJellyfin, StringComparison.Ordinal) ||
            (!string.IsNullOrWhiteSpace(localFilePath) &&
             localFilePath.StartsWith("jellyfin://", StringComparison.OrdinalIgnoreCase)))
        {
            var streamUrl = ResolveJellyfinStreamUrl(song.RemoteUrl, localFilePath);
            return string.IsNullOrWhiteSpace(streamUrl)
                ? PlaybackSourceResult.Failed(
                    PlaybackSourceFailureReason.Unavailable,
                    PlaybackSourceOrigin.Jellyfin)
                : PlaybackSourceResult.Remote(streamUrl, PlaybackSourceOrigin.Jellyfin);
        }

        if (!string.IsNullOrWhiteSpace(localFilePath) && File.Exists(localFilePath))
            return PlaybackSourceResult.Local(localFilePath);

        // QQ 音乐播放源
        if (song.PlaybackSource == SongPlaybackSource.QqCatalog)
            return await ResolveQqPlaybackAsync(song, quality, cancellationToken);

        var sourceOrigin = song.PlaybackSource == SongPlaybackSource.UserCloud
            ? PlaybackSourceOrigin.KugouUserCloud
            : PlaybackSourceOrigin.KugouCatalog;

        if (string.IsNullOrEmpty(sessionManager.Session.Token) || sessionManager.Session.UserId == "0")
            return PlaybackSourceResult.Failed(PlaybackSourceFailureReason.LoginRequired, sourceOrigin);

        if (song.PlaybackSource == SongPlaybackSource.UserCloud)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var cloudData = await userClient.GetCloudUrlAsync(
                song.Hash,
                song.AlbumAudioId > 0 ? song.AlbumAudioId.ToString() : null,
                song.AudioId > 0 ? song.AudioId.ToString() : null,
                song.Name);
            cancellationToken.ThrowIfCancellationRequested();

            if (cloudData == null || cloudData.Status != 1 || string.IsNullOrWhiteSpace(cloudData.Url))
                return PlaybackSourceResult.Failed(
                    PlaybackSourceFailureReason.Unavailable,
                    PlaybackSourceOrigin.KugouUserCloud);

            return PlaybackSourceResult.Remote(cloudData.Url, PlaybackSourceOrigin.KugouUserCloud);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var effectiveQuality = await ResolvePlaybackQualityAsync(song, quality, cancellationToken);
        var playData = await songClient.GetPlayInfoAsync(song.Hash, effectiveQuality, song.AlbumId);
        cancellationToken.ThrowIfCancellationRequested();

        if (playData == null || playData.Status != 1)
            return PlaybackSourceResult.Failed(
                PlaybackSourceFailureReason.Unavailable,
                PlaybackSourceOrigin.KugouCatalog);

        var url = playData.Urls?.AsValueEnumerable().FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        return string.IsNullOrWhiteSpace(url)
            ? PlaybackSourceResult.Failed(
                PlaybackSourceFailureReason.EmptyUrl,
                PlaybackSourceOrigin.KugouCatalog)
            : PlaybackSourceResult.Remote(url, PlaybackSourceOrigin.KugouCatalog);
    }

    /// <summary>
    ///     解析 QQ 音乐播放地址。song.Hash 存储 songmid。
    /// </summary>
    private async Task<PlaybackSourceResult> ResolveQqPlaybackAsync(
        SongItem song, string quality, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(song.Hash))
            return PlaybackSourceResult.Failed(
                PlaybackSourceFailureReason.Unavailable, PlaybackSourceOrigin.QqCatalog);

        if (!qqCookieService.IsLoggedIn)
            return PlaybackSourceResult.Failed(
                PlaybackSourceFailureReason.LoginRequired, PlaybackSourceOrigin.QqCatalog);

        cancellationToken.ThrowIfCancellationRequested();

        var qqQuality = MapQualityToQq(quality);
        var playData = await qqSongClient.GetPlayUrlAsync(song.Hash, qqQuality, qqCookieService.Cookie);
        cancellationToken.ThrowIfCancellationRequested();

        if (playData == null || !playData.IsSuccess)
        {
            logger.LogWarning(
                "QQ 音乐播放地址获取失败: {SongName} (songmid={SongMid})",
                song.Name, song.Hash);
            return PlaybackSourceResult.Failed(
                PlaybackSourceFailureReason.Unavailable, PlaybackSourceOrigin.QqCatalog);
        }

        var url = playData.GetPlayableUrl(song.Hash);

        return string.IsNullOrWhiteSpace(url)
            ? PlaybackSourceResult.Failed(PlaybackSourceFailureReason.EmptyUrl, PlaybackSourceOrigin.QqCatalog)
            : PlaybackSourceResult.Remote(url, PlaybackSourceOrigin.QqCatalog);
    }

    private static string MapQualityToQq(string quality)
    {
        return quality?.ToLowerInvariant() switch
        {
            "128" or "standard" or "标准" => "128",
            "320" or "high" or "高品质" => "320",
            "flac" or "lossless" or "无损" => "flac",
            "ape" => "ape",
            "m4a" => "m4a",
            _ => QqMusic.Net.Abstractions.QqAudioQuality.Default
        };
    }

    private async Task<string> ResolvePlaybackQualityAsync(
        SongItem song,
        string requestedQuality,
        CancellationToken cancellationToken)
    {
        var requestedRank = AudioQuality.GetRank(requestedQuality);
        if (requestedRank < 0 || string.IsNullOrWhiteSpace(song.Hash))
            return requestedQuality;

        var privileges = await songClient.GetPrivilegeLiteAsync(song.Hash, song.AlbumId);
        cancellationToken.ThrowIfCancellationRequested();
        if (privileges == null || privileges.Count == 0)
            return requestedQuality;

        var highestSupportedQualityRank = EnumerateSupportedQualities(privileges)
            .AsValueEnumerable().Select(AudioQuality.GetRank)
            .Where(rank => rank >= 0)
            .DefaultIfEmpty(-1)
            .Max();

        if (highestSupportedQualityRank < 0 || highestSupportedQualityRank >= requestedRank)
            return requestedQuality;

        var effectiveQuality = AudioQuality.Ordered[highestSupportedQualityRank];
        logger.LogInformation(
            "Playback quality downgraded for song {SongName} ({Hash}) from {RequestedQuality} to {EffectiveQuality}",
            song.Name,
            song.Hash,
            requestedQuality,
            effectiveQuality);

        return effectiveQuality;
    }

    private static IEnumerable<string> EnumerateSupportedQualities(IEnumerable<PrivilegeLiteData> privileges)
    {
        var stack = new Stack<PrivilegeLiteData>(privileges);
        var yielded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current.RelateGoods.Count > 0)
            {
                foreach (var relateGood in current.RelateGoods)
                    stack.Push(relateGood);
            }

            var normalizedQuality = AudioQuality.Normalize(current.Quality);
            if (!AudioQuality.IsKnown(normalizedQuality) || !yielded.Add(normalizedQuality))
                continue;

            yield return normalizedQuality;
        }
    }

    private static bool IsHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private string? ResolveJellyfinStreamUrl(string? cachedStreamUrl, string? source)
    {
        if (!string.IsNullOrWhiteSpace(cachedStreamUrl))
            return cachedStreamUrl;

        if (IsHttpUrl(source))
            return source;

        return string.IsNullOrWhiteSpace(source)
            ? null
            : jellyfinClient.BuildStreamUrlFromPseudoPath(source);
    }
}

public sealed record PlaybackSourceResult(
    bool Success,
    string? Source,
    bool IsLocal,
    PlaybackSourceFailureReason FailureReason,
    PlaybackSourceOrigin Origin)
{
    public static PlaybackSourceResult Local(string source) =>
        new(
            true,
            source,
            true,
            PlaybackSourceFailureReason.None,
            PlaybackSourceOrigin.LocalFile);

    public static PlaybackSourceResult Remote(string source, PlaybackSourceOrigin origin) =>
        new(true, source, false, PlaybackSourceFailureReason.None, origin);

    public static PlaybackSourceResult Failed(
        PlaybackSourceFailureReason reason,
        PlaybackSourceOrigin origin) =>
        new(false, null, false, reason, origin);
}

public enum PlaybackSourceFailureReason
{
    None,
    LoginRequired,
    Unavailable,
    EmptyUrl
}

public enum PlaybackSourceOrigin
{
    Unknown,
    LocalFile,
    Jellyfin,
    KugouUserCloud,
    KugouCatalog,
    QqCatalog
}
