using QqMusic.Net.Clients;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Raw;

namespace KgWebApi.Net.Services;

/// <summary>
///     QQ 音乐 API 服务注册扩展。
/// </summary>
public static class WebApiQqMusicServiceCollectionExtensions
{
    /// <summary>
    ///     注册 QQ 音乐 SDK 的 Raw API 层和 Client 层。
    /// </summary>
    public static IServiceCollection AddWebApiQqMusicServices(this IServiceCollection services)
    {
        // HTTP 客户端（连接到 localhost:3200 的 qq-music-api 服务）
        services.AddHttpClient<IQqTransport, QqHttpTransport>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:3200");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            UseCookies = false,
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 64
        })
        .SetHandlerLifetime(Timeout.InfiniteTimeSpan);

        // Raw API 层
        services.AddScoped<RawSearchApi>();
        services.AddScoped<RawSongApi>();
        services.AddScoped<RawLyricApi>();
        services.AddScoped<RawPlaylistApi>();
        services.AddScoped<RawRankApi>();
        services.AddScoped<RawArtistApi>();
        services.AddScoped<RawAlbumApi>();
        services.AddScoped<RawUserApi>();
        services.AddScoped<RawLoginApi>();
        services.AddScoped<RawRecommendApi>();
        services.AddScoped<RawMvApi>();
        services.AddScoped<RawCommentApi>();
        services.AddScoped<RawImageApi>();

        // Client 层
        services.AddScoped<SearchClient>();
        services.AddScoped<SongClient>();
        services.AddScoped<LyricClient>();
        services.AddScoped<PlaylistClient>();
        services.AddScoped<RankClient>();
        services.AddScoped<ArtistClient>();
        services.AddScoped<AlbumClient>();
        services.AddScoped<UserClient>();
        services.AddScoped<LoginClient>();
        services.AddScoped<RecommendClient>();
        services.AddScoped<MvClient>();
        services.AddScoped<CommentClient>();
        services.AddScoped<ImageClient>();

        return services;
    }
}
