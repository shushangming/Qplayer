using System;
using ZLinq;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KugouAvaloniaPlayer.Models;
using KugouAvaloniaPlayer.Services;
using Microsoft.Extensions.Logging;
using QqMusic.Net.Abstractions.Models;
using QqMusic.Net.Clients;
using SukiUI.Toasts;

namespace KugouAvaloniaPlayer.ViewModels;

public partial class RankItem : ObservableObject
{
    [ObservableProperty]
    public partial string Cover { get; set; } = "avares://QqMusicPlayer/Assets/Default.png";

    [ObservableProperty]
    public partial string Name { get; set; } = "";

    [ObservableProperty]
    public partial long RankId { get; set; }

    [ObservableProperty]
    public partial long Classify { get; set; }
}

public sealed class RankSection
{
    public string Title { get; init; } = "";

    public AvaloniaList<RankItem> Items { get; } = new();
}

public partial class RankViewModel : PageViewModelBase
{
    private const string DefaultCover = "avares://QqMusicPlayer/Assets/Default.png";
    private const int SongPageSize = 100;
    private readonly ILogger<RankViewModel> _logger;
    private readonly INavigationService _navigationService;
    private readonly RankClient _rankClient;
    private readonly ISukiToastManager _toastManager;

    private int _currentPage = 1;
    private bool _hasMoreSongs = true;
    private RankDetailBackTarget _detailBackTarget = RankDetailBackTarget.RankList;
    [ObservableProperty]
    public partial bool IsLoadingMore { get; set; }

    [ObservableProperty]
    public partial bool IsShowingSongs { get; set; }

    [ObservableProperty]
    public partial RankItem? SelectedRank { get; set; }

    public RankViewModel(
        RankClient rankClient,
        INavigationService navigationService,
        ISukiToastManager toastManager,
        ILogger<RankViewModel> logger)
    {
        _rankClient = rankClient;
        _navigationService = navigationService;
        _toastManager = toastManager;
        _logger = logger;
        _ = LoadAllRanks();
    }

    public override string DisplayName => "排行榜";
    public override string Icon => "/Assets/headphones-with-music-note-svgrepo-com.svg";

    public AvaloniaList<RankItem> Ranks { get; } = new();
    public AvaloniaList<RankSection> RankSections { get; } = new();
    public AvaloniaList<SongItem> SelectedRankSongs { get; } = new();

    public void ShowRankList()
    {
        _detailBackTarget = RankDetailBackTarget.RankList;
        ClearDetail();
    }

    public Task OpenRankDetailFromPreviousPageAsync(long rankId, string? fallbackName = null, string? fallbackCover = null)
    {
        if (rankId <= 0)
            return Task.CompletedTask;

        var rank = new RankItem
        {
            RankId = rankId,
            Name = fallbackName ?? "",
            Cover = string.IsNullOrWhiteSpace(fallbackCover) ? DefaultCover : fallbackCover
        };

        return OpenRankDetailAsync(rank, RankDetailBackTarget.PreviousPage);
    }

    public async Task OpenRankFromListByIdAsync(
        long rankId,
        string? fallbackName = null,
        string? fallbackCover = null)
    {
        if (rankId <= 0)
            return;

        if (Ranks.Count == 0)
            await LoadAllRanks();

        var rank = Ranks.AsValueEnumerable().FirstOrDefault(item => item.RankId == rankId)
                   ?? new RankItem
                   {
                       RankId = rankId,
                       Name = fallbackName ?? "",
                       Cover = string.IsNullOrWhiteSpace(fallbackCover) ? DefaultCover : fallbackCover
                   };

        await OpenRankDetailAsync(rank, RankDetailBackTarget.RankList);
    }

    [RelayCommand]
    private async Task LoadAllRanks()
    {
        Ranks.Clear();
        RankSections.Clear();
        try
        {
            var response = await _rankClient.GetTopListsAsync();
            if (response == null)
                return;

            // 扁平列表
            var flat = response.TopLists;
            if (flat is { Count: > 0 })
                Ranks.AddRange(flat.AsValueEnumerable().Select(MapRankItem).ToList());

            // 分组：优先使用 QQ 返回的分组结构
            var groups = response.Groups;
            if (groups is { Count: > 0 })
            {
                foreach (var group in groups)
                {
                    if (group?.TopLists is not { Count: > 0 })
                        continue;

                    var section = new RankSection
                    {
                        Title = string.IsNullOrWhiteSpace(group.GroupName) ? "其他榜单" : group.GroupName
                    };
                    section.Items.AddRange(group.TopLists.AsValueEnumerable().Select(MapRankItem).ToList());
                    if (section.Items.Count > 0)
                        RankSections.Add(section);
                }
            }

            // 没有分组结构时，退化为单个分组
            if (RankSections.Count == 0 && Ranks.Count > 0)
            {
                var section = new RankSection { Title = "排行榜" };
                section.Items.AddRange(Ranks);
                RankSections.Add(section);
            }
        }
        catch (Exception ex)
        {
            _toastManager.CreateToast()
                .OfType(NotificationType.Warning)
                .WithTitle("获取排行榜失败")
                .WithContent(ex.Message)
                .Dismiss().After(TimeSpan.FromSeconds(3))
                .Queue();
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        if (_detailBackTarget == RankDetailBackTarget.PreviousPage)
        {
            _navigationService.GoBack();
            _detailBackTarget = RankDetailBackTarget.RankList;
            return;
        }

        ClearDetail();
    }

    [RelayCommand]
    private void NavigateBack()
    {
        _navigationService.GoBack();
    }

    [RelayCommand]
    private async Task OpenRank(RankItem? item)
    {
        await OpenRankDetailAsync(item, RankDetailBackTarget.RankList);
    }

    private async Task OpenRankDetailAsync(RankItem? item, RankDetailBackTarget backTarget)
    {
        if (item is null) return;

        _detailBackTarget = backTarget;
        SelectedRank = item;
        IsShowingSongs = true;
        SelectedRankSongs.Clear();

        _currentPage = 1;
        _hasMoreSongs = true;
        IsLoadingMore = false;

        await LoadMoreSongsInternal();
    }

    private void ClearDetail()
    {
        IsShowingSongs = false;
        SelectedRank = null;
        SelectedRankSongs.Clear();
    }

    [RelayCommand]
    private async Task LoadMore()
    {
        if (IsLoadingMore || !_hasMoreSongs)
            return;

        _currentPage++;
        await LoadMoreSongsInternal();
    }

    private async Task LoadMoreSongsInternal()
    {
        if (SelectedRank == null) return;

        IsLoadingMore = true;
        try
        {
            var response = await _rankClient.GetRanksAsync(
                topId: (int)SelectedRank.RankId,
                page: _currentPage,
                limit: SongPageSize);

            var songs = response?.Songs;
            if (songs is not { Count: > 0 })
            {
                _hasMoreSongs = false;
                return;
            }

            // 分页判断：不足一页或已达到总数则无更多
            var total = response!.TotalNum ?? 0;
            if (songs.Count < SongPageSize || (total > 0 && SelectedRankSongs.Count + songs.Count >= total))
                _hasMoreSongs = false;

            var songItems = songs.AsValueEnumerable().Select(MapRankSong).ToList();
            if (songItems.Count > 0)
                SelectedRankSongs.AddRange(songItems);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "加载排行榜歌曲失败。rankId={RankId}, page={Page}", SelectedRank.RankId, _currentPage);
            _currentPage--;
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    private static RankItem MapRankItem(QqTopListInfo info)
    {
        return new RankItem
        {
            RankId = info.Id ?? 0,
            Name = string.IsNullOrWhiteSpace(info.TopTitle) ? "" : info.TopTitle,
            Cover = string.IsNullOrWhiteSpace(info.PicUrl) ? DefaultCover : info.PicUrl,
            Classify = info.Type ?? 0
        };
    }

    private static SongItem MapRankSong(QqRankSong s)
    {
        // QQ 排行榜接口（getRanks）返回的 song 对象未暴露 songmid，
        // 这里退而用 songId 作为 Hash；播放解析仍需 songmid，故该榜单歌曲可能无法直接播放。
        var albumMid = string.IsNullOrWhiteSpace(s.AlbumMid) ? null : s.AlbumMid;
        var cover = string.IsNullOrEmpty(albumMid)
            ? (string.IsNullOrWhiteSpace(s.Cover) ? DefaultCover : s.Cover)
            : $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{albumMid}.jpg";

        return new SongItem
        {
            Name = string.IsNullOrWhiteSpace(s.Title) ? "" : s.Title,
            Singer = string.IsNullOrWhiteSpace(s.SingerName) ? "未知" : s.SingerName,
            Hash = s.SongId?.ToString() ?? "",
            AlbumId = albumMid ?? "",
            AlbumName = "",
            Cover = cover,
            DurationSeconds = 0,
            PlaybackSource = SongPlaybackSource.QqCatalog
        };
    }

    private enum RankDetailBackTarget
    {
        RankList,
        PreviousPage
    }
}
