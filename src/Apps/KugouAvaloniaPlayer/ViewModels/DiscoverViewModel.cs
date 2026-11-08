using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KuGou.Net.Abstractions.Models;
using KugouAvaloniaPlayer.Models;
using KugouAvaloniaPlayer.Services;
using Microsoft.Extensions.Logging;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using SukiUI.Toasts;
using ZLinq;

namespace KugouAvaloniaPlayer.ViewModels;

public partial class DiscoverTopCard : ObservableObject
{
    [ObservableProperty]
    public partial int CardId { get; set; }

    [ObservableProperty]
    public partial string Cover { get; set; } = "avares://QqMusicPlayer/Assets/default_listcard.png";

    [ObservableProperty]
    public partial string Description { get; set; } = "";

    [ObservableProperty]
    public partial string SongSummary { get; set; } = "";

    [ObservableProperty]
    public partial string Title { get; set; } = "";

    public AvaloniaList<SongItem> Songs { get; } = [];
}

public partial class DiscoverPlaylist : ObservableObject
{
    [ObservableProperty]
    public partial string Cover { get; set; } = "avares://QqMusicPlayer/Assets/default_listcard.png";

    [ObservableProperty]
    public partial string CreatorName { get; set; } = "";

    [ObservableProperty]
    public partial string GlobalId { get; set; } = "";

    [ObservableProperty]
    public partial string Id { get; set; } = "";

    [ObservableProperty]
    public partial string Name { get; set; } = "";

    [ObservableProperty]
    public partial long PlayCount { get; set; }

    public string PlayCountText => FormatPlayCount(PlayCount);

    partial void OnPlayCountChanged(long value)
    {
        OnPropertyChanged(nameof(PlayCountText));
    }

    private static string FormatPlayCount(long value)
    {
        return value switch
        {
            >= 100_000_000 => $"{value / 100_000_000d:0.#}亿",
            >= 10_000 => $"{value / 10_000d:0.#}万",
            > 0 => value.ToString("N0", CultureInfo.CurrentCulture),
            _ => ""
        };
    }
}

public sealed class DiscoverRankSongPreview
{
    public int Index { get; init; }

    public string Name { get; init; } = "";
}

public partial class DiscoverRankCard : ObservableObject
{
    [ObservableProperty]
    public partial string Cover { get; set; } = "avares://QqMusicPlayer/Assets/default_song.png";

    [ObservableProperty]
    public partial string Name { get; set; } = "";

    [ObservableProperty]
    public partial long RankId { get; set; }

    public AvaloniaList<DiscoverRankSongPreview> TopSongs { get; } = [];
}

public partial class DiscoverViewModel : PageViewModelBase
{
    private const string DefaultCardCover = "avares://QqMusicPlayer/Assets/default_listcard.png";
    private const string DefaultSongCover = "avares://QqMusicPlayer/Assets/default_song.png";
    private const int PlaylistPageSize = 30;

    private readonly RecommendClient _discoveryClient;
    private readonly IDiscoverTagViewModelFactory _discoverTagViewModelFactory;
    private readonly ILogger<DiscoverViewModel> _logger;
    private readonly IMessenger _messenger;
    private readonly INavigationService _navigationService;
    private readonly PlaylistClient _playlistClient;
    private readonly RankClient _rankClient;
    private readonly RankViewModel _rankViewModel;
    private readonly ISukiToastManager _toastManager;
    private int _playlistLoadVersion;

    [ObservableProperty]
    public partial string DetailCover { get; set; } = DefaultCardCover;

    [ObservableProperty]
    public partial string DetailSubtitle { get; set; } = "";

    [ObservableProperty]
    public partial string DetailTitle { get; set; } = "";

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingMoreSongs { get; set; }

    [ObservableProperty]
    public partial bool IsPlaylistsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsShowingSongs { get; set; }

    [ObservableProperty]
    public partial bool IsTopCardsLoading { get; set; }

    [ObservableProperty]
    public partial DiscoverPlaylist? SelectedPlaylist { get; set; }

    [ObservableProperty]
    public partial DiscoverTopCard? SelectedTopCard { get; set; }

    public DiscoverViewModel(
        PlaylistClient playlistClient,
        RecommendClient discoveryClient,
        RankClient rankClient,
        RankViewModel rankViewModel,
        IDiscoverTagViewModelFactory discoverTagViewModelFactory,
        INavigationService navigationService,
        ISukiToastManager toastManager,
        IMessenger messenger,
        ILogger<DiscoverViewModel> logger)
    {
        _playlistClient = playlistClient;
        _discoveryClient = discoveryClient;
        _rankClient = rankClient;
        _rankViewModel = rankViewModel;
        _discoverTagViewModelFactory = discoverTagViewModelFactory;
        _navigationService = navigationService;
        _toastManager = toastManager;
        _messenger = messenger;
        _logger = logger;
        _ = LoadContentAsync();
    }

    public override string DisplayName => "探索发现";
    public override string Icon => "/Assets/tag-svgrepo-com.svg";

    public bool CanCollectSelectedPlaylist => SelectedPlaylist is not null;
    public AvaloniaList<DiscoverPlaylist> Playlists { get; } = [];
    public AvaloniaList<DiscoverRankCard> RankCards { get; } = [];
    public AvaloniaList<SongItem> SelectedDetailSongs { get; } = [];
    public AvaloniaList<SongItem> SelectedPlaylistSongs => SelectedDetailSongs;
    public AvaloniaList<DiscoverTopCard> TopCards { get; } = [];

    [ObservableProperty]
    public partial bool IsRanksLoading { get; set; }

    partial void OnSelectedPlaylistChanged(DiscoverPlaylist? value)
    {
        OnPropertyChanged(nameof(CanCollectSelectedPlaylist));
    }

    public async Task LoadContentAsync()
    {
        var version = ++_playlistLoadVersion;
        IsLoading = true;

        try
        {
            await Task.WhenAll(LoadTopCardsAsync(), LoadRecommendedPlaylistsAsync(version), LoadRankCardsAsync(version));
        }
        finally
        {
            if (version == _playlistLoadVersion)
                IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshDiscovery()
    {
        await LoadContentAsync();
    }

    [RelayCommand]
    private void OpenPlaylistTags()
    {
        _navigationService.NavigateTransient(_discoverTagViewModelFactory.Create());
    }

    [RelayCommand]
    private void OpenRanksPage()
    {
        _rankViewModel.ShowRankList();
        _navigationService.Navigate(_rankViewModel);
    }

    [RelayCommand]
    private async Task OpenRankCard(DiscoverRankCard? card)
    {
        if (card is null)
            return;

        var openTask = _rankViewModel.OpenRankDetailFromPreviousPageAsync(card.RankId, card.Name, card.Cover);
        _navigationService.Navigate(_rankViewModel);
        await openTask;
    }

    [RelayCommand]
    private void OpenTopCard(DiscoverTopCard? card)
    {
        if (card == null || card.Songs.Count == 0)
            return;

        SelectedTopCard = card;
        SelectedPlaylist = null;
        DetailTitle = card.Title;
        DetailSubtitle = string.IsNullOrWhiteSpace(card.Description)
            ? $"{card.Songs.Count} 首歌曲"
            : card.Description;
        DetailCover = string.IsNullOrWhiteSpace(card.Cover) ? DefaultCardCover : card.Cover;

        SelectedDetailSongs.Clear();
        SelectedDetailSongs.AddRange(card.Songs);
        IsShowingSongs = true;
        IsLoadingMoreSongs = false;
    }

    [RelayCommand]
    private async Task OpenPlaylist(DiscoverPlaylist? playlist)
    {
        if (playlist is null || string.IsNullOrWhiteSpace(playlist.GlobalId))
            return;

        SelectedPlaylist = playlist;
        SelectedTopCard = null;
        DetailTitle = playlist.Name;
        DetailSubtitle = string.IsNullOrWhiteSpace(playlist.CreatorName) ? "推荐歌单" : $"by {playlist.CreatorName}";
        DetailCover = string.IsNullOrWhiteSpace(playlist.Cover) ? DefaultCardCover : playlist.Cover;

        SelectedDetailSongs.Clear();
        IsShowingSongs = true;

        IsLoadingMoreSongs = false;

        await LoadPlaylistDetailInternal();
    }

    [RelayCommand]
    private void CollectPlaylist()
    {
        if (SelectedPlaylist is null || string.IsNullOrWhiteSpace(SelectedPlaylist.GlobalId))
            return;

        // QQ 音乐 SDK 当前未提供收藏歌单接口，优雅降级。
        _toastManager.CreateToast()
            .OfType(NotificationType.Warning)
            .WithTitle("暂不支持")
            .WithContent("QQ 音乐暂不支持在播放器内收藏歌单")
            .Dismiss().After(TimeSpan.FromSeconds(3))
            .Dismiss().ByClicking()
            .Queue();
    }

    [RelayCommand]
    private void GoBack()
    {
        IsShowingSongs = false;
        SelectedPlaylist = null;
        SelectedTopCard = null;
        SelectedDetailSongs.Clear();
        DetailTitle = "";
        DetailSubtitle = "";
        DetailCover = DefaultCardCover;
    }

    [RelayCommand]
    private Task LoadMoreSongs()
    {
        // QQ 歌单详情一次性返回全部歌曲，无分页。
        return Task.CompletedTask;
    }

    private async Task LoadTopCardsAsync()
    {
        IsTopCardsLoading = true;
        try
        {
            // QQ 首页推荐包返回的是歌单/专辑聚合（category 分组），没有与酷狗“专属好歌卡”
            // 对应的可直接播放的歌曲卡，这里保留空态，避免崩溃。
            TopCards.Clear();

            var recommend = await _discoveryClient.GetRecommendAsync();
            var groupCount = recommend?.Category?.Count ?? 0;
            _logger.LogInformation(
                "QQ 首页推荐包已返回 {GroupCount} 个分组，但无对应歌曲卡，TopCards 留空。", groupCount);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "加载 QQ 首页推荐包失败");
        }
        finally
        {
            IsTopCardsLoading = false;
        }
    }

    private async Task LoadRecommendedPlaylistsAsync(int version)
    {
        IsPlaylistsLoading = true;
        try
        {
            var result = await _playlistClient.GetSongListsAsync(limit: PlaylistPageSize, page: 1);
            if (version != _playlistLoadVersion)
                return;

            Playlists.Clear();

            var items = result?.List;
            if (items is not { Count: > 0 })
                return;

            Playlists.AddRange(items.AsValueEnumerable().Select(MapDiscoverPlaylist).ToList());
        }
        catch (Exception ex)
        {
            if (version == _playlistLoadVersion)
                ShowWarning("加载推荐歌单失败", ex.Message);
            _logger.LogWarning(ex, "加载推荐歌单失败");
        }
        finally
        {
            if (version == _playlistLoadVersion)
                IsPlaylistsLoading = false;
        }
    }

    private async Task LoadRankCardsAsync(int version)
    {
        IsRanksLoading = true;
        try
        {
            var response = await _rankClient.GetTopListsAsync();
            if (version != _playlistLoadVersion)
                return;

            RankCards.Clear();

            var ranks = response?.TopLists;
            if (ranks is not { Count: > 0 })
                return;

            var cards = ranks.AsValueEnumerable()
                .Select(MapRankCard)
                .Where(card => !string.IsNullOrWhiteSpace(card.Name))
                .Take(6)
                .ToList();

            RankCards.AddRange(cards);
        }
        catch (Exception ex)
        {
            if (version == _playlistLoadVersion)
                _logger.LogWarning(ex, "加载排行榜卡片失败");
        }
        finally
        {
            if (version == _playlistLoadVersion)
                IsRanksLoading = false;
        }
    }

    private async Task LoadPlaylistDetailInternal()
    {
        if (SelectedPlaylist == null)
            return;

        IsLoadingMoreSongs = true;
        try
        {
            var detail = await _playlistClient.GetSongListDetailAsync(SelectedPlaylist.GlobalId);
            if (detail is null)
            {
                _logger.LogWarning("歌单详情返回为空。disstid={Disstid}", SelectedPlaylist.GlobalId);
                return;
            }

            var songs = detail.SongList;
            if (songs is not { Count: > 0 })
                return;

            var songItems = songs.AsValueEnumerable().Select(MapQqSong).ToList();
            if (songItems.Count > 0)
                SelectedDetailSongs.AddRange(songItems);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "加载歌单详情失败。disstid={Disstid}", SelectedPlaylist.GlobalId);
        }
        finally
        {
            IsLoadingMoreSongs = false;
        }
    }

    private static DiscoverPlaylist MapDiscoverPlaylist(QqPlaylistLite item)
    {
        return new DiscoverPlaylist
        {
            Id = item.DissId ?? "",
            GlobalId = item.DissId ?? "",
            Name = string.IsNullOrWhiteSpace(item.DissName) ? "" : item.DissName,
            Cover = string.IsNullOrWhiteSpace(item.ImgUrl) ? DefaultCardCover : item.ImgUrl,
            PlayCount = item.ListenNum ?? 0,
            CreatorName = item.Creator?.Name ?? ""
        };
    }

    private static DiscoverRankCard MapRankCard(QqTopListInfo info)
    {
        var card = new DiscoverRankCard
        {
            RankId = info.Id ?? 0,
            Name = string.IsNullOrWhiteSpace(info.TopTitle) ? "" : info.TopTitle,
            Cover = string.IsNullOrWhiteSpace(info.PicUrl) ? DefaultSongCover : info.PicUrl
        };

        var topSongs = (info.SongList ?? new List<QqTopListSongPreview>())
            .AsValueEnumerable()
            .Select(song => string.IsNullOrWhiteSpace(song.SongName) ? song.SingerName : song.SongName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Take(3)
            .Select((name, index) => new DiscoverRankSongPreview
            {
                Index = index + 1,
                Name = name!
            })
            .ToList();

        card.TopSongs.AddRange(topSongs);
        return card;
    }

    private static SongItem MapQqSong(QqSongInfo s)
    {
        var albumMid = string.IsNullOrWhiteSpace(s.AlbumMid) ? null : s.AlbumMid;
        var cover = string.IsNullOrEmpty(albumMid)
            ? DefaultSongCover
            : $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{albumMid}.jpg";

        var singers = s.Singers?
            .AsValueEnumerable()
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new SingerLite { Id = x.Id ?? 0, Name = x.Name })
            .ToList() ?? new List<SingerLite>();

        return new SongItem
        {
            Name = string.IsNullOrWhiteSpace(s.SongName) ? "" : s.SongName,
            Singer = singers.Count > 0
                ? string.Join("、", singers.AsValueEnumerable().Select(x => x.Name).ToArray())
                : "未知",
            Hash = s.SongMid ?? "",
            AlbumId = albumMid ?? "",
            AlbumName = s.AlbumName ?? "",
            Cover = cover,
            DurationSeconds = s.Interval,
            Singers = singers,
            PlaybackSource = SongPlaybackSource.QqCatalog
        };
    }

    private void ShowWarning(string title, string content)
    {
        _toastManager.CreateToast()
            .OfType(NotificationType.Warning)
            .WithTitle(title)
            .WithContent(content)
            .Dismiss().After(TimeSpan.FromSeconds(3))
            .Dismiss().ByClicking()
            .Queue();
    }
}
