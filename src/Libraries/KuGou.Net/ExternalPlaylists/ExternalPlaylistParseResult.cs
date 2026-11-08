namespace KuGou.Net.ExternalPlaylists;

public sealed class ExternalPlaylistParseResult
{
    public bool Success { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public string SourcePlatform { get; init; } = string.Empty;
    public string SourcePlaylistName { get; init; } = string.Empty;
    public List<string> SongNames { get; init; } = new();
}