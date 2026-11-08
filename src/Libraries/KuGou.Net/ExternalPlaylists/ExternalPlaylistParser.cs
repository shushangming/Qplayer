using System.Text.RegularExpressions;

namespace KuGou.Net.ExternalPlaylists;

public sealed class ExternalPlaylistParser(IEnumerable<IExternalPlaylistParseStrategy> strategies)
    : IExternalPlaylistParser
{
    private static readonly Regex UrlRegex = new(@"https?://[^\s]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly List<IExternalPlaylistParseStrategy> _strategies = strategies.ToList();

    public async Task<ExternalPlaylistParseResult> ParseAndLoadAsync(
        string sourceText,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceText))
            return new ExternalPlaylistParseResult { ErrorMessage = "链接不能为空。" };

        var url = ExtractUrl(sourceText);
        if (string.IsNullOrWhiteSpace(url))
            return new ExternalPlaylistParseResult { ErrorMessage = "未识别到有效链接。" };

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return new ExternalPlaylistParseResult { ErrorMessage = "链接格式不正确。" };

        var strategy = _strategies.FirstOrDefault(x => x.CanHandle(uri));
        if (strategy == null)
            return new ExternalPlaylistParseResult { ErrorMessage = "暂只支持网易云和QQ音乐歌单链接。" };

        return await strategy.ParseAndLoadAsync(uri, sourceText, cancellationToken);
    }

    private static string? ExtractUrl(string text)
    {
        var match = UrlRegex.Match(text);
        return match.Success ? match.Value.Trim() : text.Trim();
    }
}