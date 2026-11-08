using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KuGou.Net.Abstractions.Models;
using KugouAvaloniaPlayer.Models;
using KugouAvaloniaPlayer.Services;
using Microsoft.Extensions.Logging;
using QqMusic.Net.Abstractions.Models;
using SukiUI.Toasts;
using QqAlbumClient = QqMusic.Net.Clients.AlbumClient;
using QqPlaylistClient = QqMusic.Net.Clients.PlaylistClient;
using QqSearchClient = QqMusic.Net.Clients.SearchClient;

namespace KugouAvaloniaPlayer.ViewModels;

public partial class SearchViewModel(
    QqSearchClient searchClient,
    QqPlaylistClient playlistClient,
    QqAlbumClient albumClient,
    INavigationService navigationService,
    PlayerViewModel player,
    ISongInteractionService songInteractions,
    ISukiToastManager toastManager,
    ILogger<SearchViewModel> logger) : PageViewModelBase
{
    private const string DefaultSongCover = "avares://QqMusicPlayer/Assets/default_song.png";
    private const string DefaultCardCover = "avares://QqMusicPlayer/Assets/default_listcard.png";
    private const string DefaultSingerCover = "avares://QqMusicPlayer/Assets/default_singer.png";
    private const int SearchPageSize = 50;
    private const int AlbumSongPageSize = 50;
    private string _currentDetailId = "";
    private long _currentAlbumId;

    private int _currentDetailPage = 1;
    private DetailType _currentDetailType = DetailType.None;

    // 用于收藏歌单的信息
    private string _currentPlaylistGlobalId = "";
    private string _currentPlaylistName = "";
    [ObservableProperty]
    public partial SearchType CurrentSearchType { get; set; } = SearchType.Song;

    [ObservableProperty]
    public partial string? DetailCover { get; set; }

    [ObservableProperty]
    public partial string? DetailSubTitle { get; set; }

    [ObservableProperty]
    public partial string? DetailTitle { get; set; }

    private bool _hasMoreDetails = true;
    [ObservableProperty]
    public partial bool HasSearched { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingMoreDetails { get; set; }

    [ObservableProperty]
    public partial bool IsSearching { get; set; }

    [ObservableProperty]
    public partial bool IsShowingDetail { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    public partial string SearchKeyword { get; set; } = "";

    public override string DisplayName => "搜索";
    public override string Icon => "/Assets/Search.svg";

    public AvaloniaList<SongItem> Songs { get; } = new();
    public AvaloniaList<SearchPlaylistItem> Playlists { get; } = new();
    public AvaloniaList<SearchAlbumItem> Albums { get; } = new();
    public AvaloniaList<SearchAuthorItem> Singers { get; } = new();
    public AvaloniaList<SongItem> DetailSongs { get; } = new();

    [RelayCommand]
    private Task PlaySong(SongItem? song) =>
        song is null
            ? Task.CompletedTask
            : ((IPlaybackCommands)player).PlayAsync(song, Songs);

    [RelayCommand]
    private void AddSongToNext(SongItem? song)
    {
        if (song != null)
            ((IPlaybackCommands)player).AddToNext(song);
    }

    [RelayCommand]
    private Task ShowSongPlaylistDialog(SongItem? song) =>
        song is null
            ? Task.CompletedTask
            : songInteractions.ShowAddToPlaylistDialogAsync(song);

    [RelayCommand]
    private void ViewSongSinger(SingerLite? singer)
    {
        if (singer != null)
            songInteractions.NavigateToSinger(singer);
    }

    // 当前是否显示歌单详情（用于控制收藏按钮可见性）
    public bool IsPlaylistDetail => _currentDetailType == DetailType.Playlist;
    public bool IsAlbumDetail => _currentDetailType == DetailType.Album;
    public bool CanCollectDetail => IsPlaylistDetail || IsAlbumDetail;

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchKeyword)) return;

        HasSearched = true;
        IsShowingDetail = false;

        IsSearching = true;
        logger.LogInformation("正在搜索: {Keyword}, 类型: {Type}", SearchKeyword, CurrentSearchType);

        ClearResults();

        try
        {
            switch (CurrentSearchType)
            {
                case SearchType.Song:
                    await SearchSongs();
                    break;
                case SearchType.Playlist:
                    await SearchPlaylists();
                    break;
                case SearchType.Album:
                    await SearchAlbums();
                    break;
                case SearchType.Singer:
                    await SearchSingers();
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "搜索失败");
        }
        finally
        {
            IsSearching = false;
        }
    }

    private void ClearResults()
    {
        Songs.Clear();
        Playlists.Clear();
        Albums.Clear();
        Singers.Clear();
    }

    private async Task SearchSongs()
    {
        var result = await searchClient.SearchAsync(SearchKeyword, SearchPageSize, 1, "song");
        var list = result?.Song?.List;
        if (list == null) return;

        foreach (var song in list)
            Songs.Add(ConvertQqSong(song));
    }

    private async Task SearchPlaylists()
    {
        for (var page = 1; ; page++)
        {
            try
            {
                var result = await searchClient.SearchAsync(SearchKeyword, SearchPageSize, page, "playlist");
                var list = result?.Playlist?.List;
                if (list == null || list.Count == 0) break;

                foreach (var item in list)
                    Playlists.Add(new SearchPlaylistItem
                    {
                        Name = item.DissName ?? "",
                        GlobalId = item.DissId,
                        SongCount = 0,
                        CreatorName = item.Creator?.Name ?? "",
                        PlayCount = item.ListenNum ?? 0,
                        Cover = string.IsNullOrWhiteSpace(item.ImgUrl) ? DefaultCardCover : item.ImgUrl
                    });

                if (list.Count < SearchPageSize) break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "网络问题，歌单搜索中断");
                break;
            }
        }
    }

    private async Task SearchAlbums()
    {
        for (var page = 1; ; page++)
        {
            try
            {
                var result = await searchClient.SearchAsync(SearchKeyword, SearchPageSize, page, "album");
                var list = result?.Album?.List;
                if (list == null || list.Count == 0) break;

                foreach (var item in list)
                {
                    var cover = !string.IsNullOrWhiteSpace(item.AlbumPic)
                        ? item.AlbumPic
                        : item.AlbumPic2;
                    Albums.Add(new SearchAlbumItem
                    {
                        AlbumId = item.AlbumId ?? 0,
                        Name = item.AlbumName ?? item.AlbumName2 ?? "",
                        SingerName = item.SingerName ?? item.SingerName2
                            ?? item.Singers?.FirstOrDefault()?.Name ?? "",
                        SongCount = item.SongCount ?? item.SongCount2 ?? 0,
                        PublishTime = item.PublicTime ?? item.PublicTime2 ?? "",
                        Cover = string.IsNullOrWhiteSpace(cover) ? DefaultCardCover : cover
                    });
                }

                if (list.Count < SearchPageSize) break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "网络问题，专辑搜索中断");
                break;
            }
        }
    }

    private async Task SearchSingers()
    {
        try
        {
            var result = await searchClient.SearchAsync(SearchKeyword, SearchPageSize, 1, "singer");

            // 上游 v2.6.0 的 getSearchByKey 不返回 singer 分区（只返回 song + zhida），
            // 优先用 zhida.zhida_singer（关键字命中歌手时的直达歌手信息）。
            var zhida = result?.Zhida?.ZhidaSinger;
            if (zhida is { SingerMid: not null })
            {
                var zhidaCover = string.IsNullOrWhiteSpace(zhida.SingerPic)
                    ? DefaultSingerCover
                    : zhida.SingerPic;
                Singers.Add(new SearchAuthorItem
                {
                    AuthorId = zhida.SingerId ?? 0,
                    Name = zhida.SingerName ?? "",
                    AudioCount = zhida.SongNum ?? 0,
                    Heat = zhida.AlbumNum ?? 0,
                    PublishTime = "",
                    Cover = zhidaCover
                });
                return;
            }

            var list = result?.Singer?.List;
            if (list == null || list.Count == 0)
                return;

            foreach (var item in list)
            {
                var cover = !string.IsNullOrWhiteSpace(item.SingerPic)
                    ? item.SingerPic
                    : item.Pic;
                Singers.Add(new SearchAuthorItem
                {
                    AuthorId = item.SingerId ?? 0,
                    Name = item.SingerName ?? item.Name ?? "",
                    AudioCount = item.SongNum ?? 0,
                    Heat = item.Fans ?? 0,
                    PublishTime = "",
                    Cover = string.IsNullOrWhiteSpace(cover) ? DefaultSingerCover : cover
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "网络问题，歌手搜索失败");
        }
    }

    [RelayCommand]
    private void SwitchSearchType(string type)
    {
        if (!Enum.TryParse<SearchType>(type, out var searchType)) return;
        CurrentSearchType = searchType;
        IsShowingDetail = false;
        ClearResults();
        if (!string.IsNullOrWhiteSpace(SearchKeyword)) _ = Search();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        ResetSearchState();
        navigationService.GoBack();
    }

    private void ResetSearchState()
    {
        SearchKeyword = string.Empty;
        IsSearching = false;
        IsShowingDetail = false;
        HasSearched = false;

        _currentDetailType = DetailType.None;
        _currentDetailId = string.Empty;
        _currentDetailPage = 1;
        _hasMoreDetails = true;
        DetailTitle = null;
        DetailSubTitle = null;
        DetailCover = null;
        _currentAlbumId = 0;
        _currentPlaylistGlobalId = string.Empty;
        _currentPlaylistName = string.Empty;

        ClearResults();
        DetailSongs.Clear();
        OnPropertyChanged(nameof(IsPlaylistDetail));
        OnPropertyChanged(nameof(IsAlbumDetail));
        OnPropertyChanged(nameof(CanCollectDetail));
    }

    [RelayCommand]
    private async Task OpenPlaylist(SearchPlaylistItem? item)
    {
        if (item == null) return;

        // 初始化状态
        _currentDetailType = DetailType.Playlist;
        OnPropertyChanged(nameof(IsPlaylistDetail));
        OnPropertyChanged(nameof(IsAlbumDetail));
        OnPropertyChanged(nameof(CanCollectDetail));
        _currentDetailId = item.GlobalId;
        _currentDetailPage = 1;
        _hasMoreDetails = true;

        // 保存收藏歌单所需的信息
        _currentPlaylistGlobalId = item.GlobalId;
        _currentPlaylistName = item.Name;

        DetailTitle = item.Name;
        DetailSubTitle = $"{item.SongCount} 首歌曲 - {item.CreatorName}";
        DetailCover = item.Cover ?? DefaultCardCover;

        IsShowingDetail = true;
        DetailSongs.Clear();

        await LoadMoreDetailsInternal();
    }

    [RelayCommand]
    private async Task OpenAlbum(SearchAlbumItem? item)
    {
        if (item == null) return;

        _currentDetailType = DetailType.Album;
        OnPropertyChanged(nameof(IsPlaylistDetail));
        OnPropertyChanged(nameof(IsAlbumDetail));
        OnPropertyChanged(nameof(CanCollectDetail));
        _currentDetailId = item.AlbumId.ToString();
        _currentAlbumId = item.AlbumId;
        _currentDetailPage = 1;
        _hasMoreDetails = true;

        DetailTitle = item.Name;
        DetailSubTitle = $"{item.SingerName}";
        DetailCover = item.Cover ?? DefaultCardCover;

        IsShowingDetail = true;
        DetailSongs.Clear();

        await LoadMoreDetailsInternal();
    }

    [RelayCommand]
    private void OpenSinger(SearchAuthorItem? item)
    {
        if (item is not { AuthorId: > 0 })
            return;

        songInteractions.NavigateToSinger(new SingerLite
        {
            Id = item.AuthorId,
            Name = item.Name,
            SingerPic = string.IsNullOrWhiteSpace(item.Cover) ? DefaultSingerCover : item.Cover
        });
    }

    [RelayCommand]
    private async Task LoadMoreDetails()
    {
        if (IsLoadingMoreDetails || !_hasMoreDetails || !IsShowingDetail)
            return;


        _currentDetailPage++;
        await LoadMoreDetailsInternal();
    }

    private async Task LoadMoreDetailsInternal()
    {
        IsLoadingMoreDetails = true;
        try
        {
            if (_currentDetailType == DetailType.Playlist)
            {
                // QQ 歌单详情一次性返回全部歌曲，仅首页加载
                if (_currentDetailPage > 1)
                {
                    _hasMoreDetails = false;
                    return;
                }

                var detail = await playlistClient.GetSongListDetailAsync(_currentDetailId);
                if (detail == null)
                {
                    logger.LogWarning("Playlist detail response is null. detailId={DetailId}",
                        _currentDetailId);
                    return;
                }

                _hasMoreDetails = false;
                var songs = detail.SongList;
                if (songs is not { Count: > 0 }) return;

                var songItems = songs.Select(ConvertQqSong).ToList();
                DetailSongs.AddRange(songItems);
            }
            else if (_currentDetailType == DetailType.Album)
            {
                var begin = (_currentDetailPage - 1) * AlbumSongPageSize;
                var detail = await albumClient.GetAlbumSongsAsync(
                    albumid: _currentAlbumId,
                    begin: begin,
                    limit: AlbumSongPageSize);

                var songs = detail?.Songs;
                if (songs == null || songs.Count < AlbumSongPageSize)
                    _hasMoreDetails = false;

                if (songs is { Count: > 0 })
                {
                    var songItems = songs.Select(ConvertQqSong).ToList();
                    DetailSongs.AddRange(songItems);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "加载详情失败");
            if (_currentDetailPage > 1) _currentDetailPage--;
        }
        finally
        {
            IsLoadingMoreDetails = false;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        IsShowingDetail = false;
        DetailSongs.Clear();
    }

    public async Task SearchAsync(string keyword)
    {
        SearchKeyword = keyword;
        await Search();
    }

    /// <summary>
    ///     将 QQ 歌曲模型映射为播放器 UI 模型。
    ///     关键约定：Hash 存 songmid，PlaybackSource 标记为 QqCatalog，播放解析依赖这两个字段。
    /// </summary>
    private static SongItem ConvertQqSong(QqSongInfo song)
    {
        var singers = song.Singers;
        var singerNames = singers?
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => x.Name)
            .ToList();
        var singerText = singerNames is { Count: > 0 }
            ? string.Join("、", singerNames)
            : "未知";

        var cover = string.IsNullOrWhiteSpace(song.AlbumMid)
            ? null
            : $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{song.AlbumMid}.jpg";

        return new SongItem
        {
            Name = string.IsNullOrWhiteSpace(song.SongName) ? "未知" : song.SongName,
            Singer = singerText,
            Hash = song.SongMid,
            AlbumId = song.AlbumMid ?? "",
            AlbumName = song.AlbumName ?? "",
            Singers = singers?
                .Select(x => new SingerLite { Id = x.Id ?? 0, Name = x.Name ?? "", SingerPic = "" })
                .ToList() ?? new List<SingerLite>(),
            Cover = string.IsNullOrWhiteSpace(cover) ? DefaultSongCover : cover,
            DurationSeconds = song.Interval,
            PlaybackSource = SongPlaybackSource.QqCatalog
        };
    }

    [RelayCommand]
    private void CollectPlaylist()
    {
        // QQ 音乐 SDK 暂无收藏歌单/专辑接口，此处改为 no-op，仅记录日志并提示。
        var name = _currentDetailType == DetailType.Playlist ? _currentPlaylistName : DetailTitle;
        logger.LogInformation("QQ 音乐目录暂不支持收藏: {Name}", name);

        toastManager.CreateToast()
            .OfType(NotificationType.Information)
            .WithTitle("暂不支持")
            .WithContent("QQ 音乐目录暂不支持收藏歌单/专辑")
            .Dismiss().After(TimeSpan.FromSeconds(3))
            .Dismiss().ByClicking()
            .Queue();
    }
}
