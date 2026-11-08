namespace KuGou.Net.ExternalPlaylists;

public interface IExternalPlaylistParseStrategy
{
    string PlatformName { get; }
    bool CanHandle(Uri uri);

    Task<ExternalPlaylistParseResult> ParseAndLoadAsync(
        Uri uri,
        string sourceText,
        CancellationToken cancellationToken = default);
}