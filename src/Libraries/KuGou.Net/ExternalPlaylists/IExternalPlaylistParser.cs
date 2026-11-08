namespace KuGou.Net.ExternalPlaylists;

public interface IExternalPlaylistParser
{
    Task<ExternalPlaylistParseResult> ParseAndLoadAsync(
        string sourceText,
        CancellationToken cancellationToken = default);
}