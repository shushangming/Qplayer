using QqMusic.Net.Clients;
using QqMusic.Net.Infrastructure.Http;
using QqMusic.Net.Protocol.Raw;
using Pure.DI;
using static Pure.DI.Lifetime;

namespace QqMusic.Net.Infrastructure;

/// <summary>
///     QQ 音乐 API Pure.DI 组合根。
/// </summary>
public sealed partial class QqMusicComposition
{
    [System.Diagnostics.Conditional("DI")]
    private static void Setup() => DI.Setup()
        .Hint(Hint.Resolve, "Off")
        .Root<QqMusicApi>(nameof(Root))
        .Bind<QqMusicApi>().As(Singleton).To<QqMusicApi>()

        .Bind<HttpClient>().As(Singleton).To(_ => new HttpClient())
        .Bind<IQqTransport>().As(Singleton).To<QqHttpTransport>()

        // Raw API 层
        .Bind<RawSearchApi>().To<RawSearchApi>()
        .Bind<RawSongApi>().To<RawSongApi>()
        .Bind<RawLyricApi>().To<RawLyricApi>()
        .Bind<RawPlaylistApi>().To<RawPlaylistApi>()
        .Bind<RawRankApi>().To<RawRankApi>()
        .Bind<RawArtistApi>().To<RawArtistApi>()
        .Bind<RawAlbumApi>().To<RawAlbumApi>()
        .Bind<RawUserApi>().To<RawUserApi>()
        .Bind<RawLoginApi>().To<RawLoginApi>()
        .Bind<RawRecommendApi>().To<RawRecommendApi>()
        .Bind<RawMvApi>().To<RawMvApi>()
        .Bind<RawCommentApi>().To<RawCommentApi>()
        .Bind<RawImageApi>().To<RawImageApi>()

        // Client 层
        .Bind<SearchClient>().To<SearchClient>()
        .Bind<SongClient>().To<SongClient>()
        .Bind<LyricClient>().To<LyricClient>()
        .Bind<PlaylistClient>().To<PlaylistClient>()
        .Bind<RankClient>().To<RankClient>()
        .Bind<ArtistClient>().To<ArtistClient>()
        .Bind<AlbumClient>().To<AlbumClient>()
        .Bind<UserClient>().To<UserClient>()
        .Bind<LoginClient>().To<LoginClient>()
        .Bind<RecommendClient>().To<RecommendClient>()
        .Bind<MvClient>().To<MvClient>()
        .Bind<CommentClient>().To<CommentClient>()
        .Bind<ImageClient>().To<ImageClient>();
}

/// <summary>
///     QQ 音乐 API 聚合入口，供外部注入使用。
/// </summary>
public readonly record struct QqMusicApi(
    SearchClient Search,
    SongClient Song,
    LyricClient Lyric,
    PlaylistClient Playlist,
    RankClient Rank,
    ArtistClient Artist,
    AlbumClient Album,
    UserClient User,
    LoginClient Login,
    RecommendClient Recommend,
    MvClient Mv,
    CommentClient Comment,
    ImageClient Image);
