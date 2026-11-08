using System;
using System.Collections;
using System.Collections.Generic;
using ZLinq;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
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

namespace KugouAvaloniaPlayer.ViewModels;

public partial class MyPlaylistsViewModel : PageViewModelBase, IDisposable
{
    private const string DefaultSortText = "默认排序";
    private const string ArtistSortText = "按歌手排序";
    private const string AlbumSortText = "按专辑排序";
    private const string ManualSortText = "手动排序";
    private const int UserPlaylistLimit = 100;
    private const string DefaultCover = "avares://QqMusicPlayer/Assets/default_listcard.png";
    private const string DefaultSongCover = "avares://QqMusicPlayer/Assets/default_song.png";
    private const string LikedPlaylistMarker = "qq:liked";

    private readonly INavigationService _navigationService;
    private readonly IMessenger _messenger;
    private readonly ILogger<MyPlaylistsViewModel> _logger;
    private readonly PlaylistClient _playlistClient;
    private readonly ISukiToastManager _toastManager;
    private readonly UserClient _userClient;
    private readonly QqCookieService _qqCookieService;
    private readonly QqLocalPlaylistService _localPlaylistService;
    private readonly ICreatePlaylistDialogService _createPlaylistDialogService;
    private readonly IExternalPlaylistImportService _externalPlaylistImportService;
    private readonly FavoritePlaylistService _favoritePlaylistService;
    private readonly SemaphoreSlim _playlistLoadGate = new(1, 1);

    private int _playlistLoadVersion;
    private int _playlistListLoadVersion;
    private bool _isDisposed;
    private readonly List<SongItem> _selectedPlaylistSongsDefaultOrder = new();
    private List<SongItem>? _onlineOriginalOrder;
    private long? _likedPlaylistId;

    [ObservableProperty]
    public partial bool IsImportingExternalPlaylist { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingMore { get; set; }

    [ObservableProperty]
    public partial bool IsShowingSongs { get; set; }

    [ObservableProperty]
    public partial string CurrentSortText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOnlinePlaylist))]
    public partial PlaylistItem? SelectedPlaylist { get; set; }

    public MyPlaylistsViewModel(
        UserClient userClient,
        PlaylistClient playlistClient,
        QqCookieService qqCookieService,
        QqLocalPlaylistService localPlaylistService,
        ICreatePlaylistDialogService createPlaylistDialogService,
        IExternalPlaylistImportService externalPlaylistImportService,
        FavoritePlaylistService favoritePlaylistService,
        ISukiToastManager toastManager,
        INavigationService navigationService,
        IMessenger messenger,
        ILogger<MyPlaylistsViewModel> logger)
    {
        _userClient = userClient;
        _playlistClient = playlistClient;
        _qqCookieService = qqCookieService;
        _localPlaylistService = localPlaylistService;
        _createPlaylistDialogService = createPlaylistDialogService;
        _externalPlaylistImportService = externalPlaylistImportService;
        _favoritePlaylistService = favoritePlaylistService;
        _toastManager = toastManager;
        _navigationService = navigationService;
        _messenger = messenger;
        _logger = logger;
        CurrentSortText = GetSortText(SettingsManager.Settings.UserPlaylistSongSortMode);

        _ = ReloadAllPlaylistsAsync();
    }

    // 标识当前选中的歌单是否为网络歌单
    public bool IsOnlinePlaylist => SelectedPlaylist?.Type == PlaylistType.Online;

    public override string DisplayName => "我的歌单";
    public override string Icon => "/Assets/music-player-svgrepo-com.svg";

    public IReadOnlyList<string> SortOptions { get; } = [DefaultSortText, ArtistSortText, AlbumSortText, ManualSortText];

    public AvaloniaList<PlaylistItem> Playlists { get; } = new();

    public AvaloniaList<SongItem> SelectedPlaylistSongs { get; } = new();

    public AvaloniaList<PlaylistItem> Items { get; } = new();

    partial void OnCurrentSortTextChanged(string value)
    {
        if (_isDisposed)
            return;

        if (string.IsNullOrWhiteSpace(value))
        {
            CurrentSortText = GetSortText(SettingsManager.Settings.UserPlaylistSongSortMode);
            return;
        }

        SettingsManager.Settings.UserPlaylistSongSortMode = GetSortMode(value);
        SettingsManager.Save();
        ApplySongSort();
    }

    [RelayCommand]
    private void GoBack()
    {
        IsShowingSongs = false;
        SelectedPlaylist = null;
        _selectedPlaylistSongsDefaultOrder.Clear();
        _onlineOriginalOrder = null;
        SelectedPlaylistSongs.Clear();
    }

    [RelayCommand]
    private Task LoadAllPlaylists()
    {
        return ReloadAllPlaylistsAsync();
    }

    /// <summary>
    ///     重新构建完整歌单快照，并在 UI 线程一次性应用，避免分批加载期间歌单闪失。
    /// </summary>
    public async Task ReloadAllPlaylistsAsync()
    {
        if (_isDisposed)
            return;

        var version = Interlocked.Increment(ref _playlistListLoadVersion);
        await _playlistLoadGate.WaitAsync();
        try
        {
            if (_isDisposed || version != _playlistListLoadVersion)
                return;

            var snapshot = new List<PlaylistItem>();
            var hiddenKeys = await _localPlaylistService.GetHiddenPlaylistKeysAsync();

            // 1. 本地歌单（无需登录，始终可用）
            var localPlaylists = await _localPlaylistService.GetAllAsync();
            foreach (var p in localPlaylists)
            {
                snapshot.Add(new PlaylistItem
                {
                    Name = p.Name,
                    Id = p.Id.ToString(),
                    ListId = p.Id,
                    Count = 0,
                    Type = PlaylistType.Local,
                    Cover = string.IsNullOrWhiteSpace(p.Cover) ? DefaultCover : p.Cover,
                    Subtitle = "本地歌单"
                });
            }

            // 未登录（无 cookie）时仅显示本地歌单，优雅降级
            if (!_qqCookieService.IsLoggedIn)
            {
                _logger.LogInformation("QQ 音乐未登录，在线歌单留空（本地歌单已加载）。");
                await ApplyPlaylistSnapshotAsync(snapshot, version);
                return;
            }

            var cookie = _qqCookieService.Cookie;

            // 解析当前登录 uin（直接从 cookie 解析，避免依赖服务端配置）
            var uin = ResolveUinAsync(cookie);
            if (uin <= 0)
            {
                _logger.LogWarning("无法解析 QQ 登录 uin，在线歌单留空。");
                await ApplyPlaylistSnapshotAsync(snapshot, version);
                return;
            }

            // 2. "我喜欢的音乐"入口（固定置顶，IsLikePlaylist=true）
            var likedItem = new PlaylistItem
            {
                Name = "我喜欢的音乐",
                Id = LikedPlaylistMarker,
                ListId = 0,
                Count = 0,
                Type = PlaylistType.Online,
                Cover = "https://y.gtimg.cn/music/photo_new/T002R300x300M000001xpUdt2jEjWu_1.jpg",
                Subtitle = "QQ 音乐 · 我喜欢的",
                IsLikePlaylist = true
            };
            if (!hiddenKeys.Contains(QqLocalPlaylistService.GetPlaylistHideKey(likedItem)))
                snapshot.Add(likedItem);

            // 3. QQ 用户创建的歌单
            var result = await _userClient.GetUserPlaylistsAsync(uin, offset: 0, limit: UserPlaylistLimit, cookie);
            if (_isDisposed)
                return;

            var lists = result?.List ?? result?.Playlists;
            if (lists is not { Count: > 0 })
            {
                _logger.LogInformation("QQ 用户歌单为空。uin={Uin}", uin);
                await ApplyPlaylistSnapshotAsync(snapshot, version);
                return;
            }

            for (var index = 0; index < lists.Count; index++)
            {
                var item = lists[index];
                var playlistItem = new PlaylistItem
                {
                    Name = string.IsNullOrWhiteSpace(item.DisplayName) ? "" : item.DisplayName,
                    Id = item.DissId ?? "",
                    ListId = long.TryParse(item.DissId, out var lid) ? lid : 0,
                    Count = 0,
                    Type = PlaylistType.Online,
                    Cover = string.IsNullOrWhiteSpace(item.DisplayCover)
                        ? DefaultCover
                        : item.DisplayCover,
                    Subtitle = item.Subtitle ?? item.Creator?.Name ?? ""
                };

                if (!hiddenKeys.Contains(QqLocalPlaylistService.GetPlaylistHideKey(playlistItem)))
                    snapshot.Add(playlistItem);
            }

            await ApplyPlaylistSnapshotAsync(snapshot, version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载 QQ 我的歌单失败");
        }
        finally
        {
            _playlistLoadGate.Release();
        }
    }

    private async Task ApplyPlaylistSnapshotAsync(List<PlaylistItem> snapshot, int version)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_isDisposed || version != _playlistListLoadVersion)
                return;

            // 复用已有实例，避免侧栏在刷新时整体重建。
            for (var i = 0; i < snapshot.Count; i++)
            {
                var incoming = snapshot[i];
                var existing = Items.AsValueEnumerable()
                    .FirstOrDefault(item => IsSamePlaylistIdentity(item, incoming));
                if (existing is null)
                    continue;

                CopyPlaylistState(existing, incoming);
                snapshot[i] = existing;
            }

            var previousSelection = SelectedPlaylist;
            Items.Clear();
            Items.AddRange(snapshot);

            if (previousSelection is null)
                return;

            var replacement = snapshot.AsValueEnumerable()
                .FirstOrDefault(item => IsSamePlaylistIdentity(item, previousSelection));
            if (replacement is not null)
            {
                SelectedPlaylist = replacement;
            }
            else if (IsShowingSongs)
            {
                GoBack();
            }
        });
    }

    private static bool IsSamePlaylistIdentity(PlaylistItem a, PlaylistItem b)
    {
        if (a.IsLikePlaylist || b.IsLikePlaylist)
            return a.IsLikePlaylist && b.IsLikePlaylist;

        if (a.Type != b.Type)
            return false;

        return a.Type switch
        {
            PlaylistType.Local => a.ListId > 0 && a.ListId == b.ListId,
            PlaylistType.Online or PlaylistType.Album => !string.IsNullOrWhiteSpace(a.Id)
                ? string.Equals(a.Id, b.Id, StringComparison.OrdinalIgnoreCase)
                : a.ListId > 0 && a.ListId == b.ListId,
            _ => false
        };
    }

    private static void CopyPlaylistState(PlaylistItem target, PlaylistItem source)
    {
        target.Name = source.Name;
        target.Id = source.Id;
        target.ListId = source.ListId;
        target.Count = source.Count;
        target.Type = source.Type;
        target.Cover = source.Cover;
        target.Subtitle = source.Subtitle;
        target.UserPlaylistType = source.UserPlaylistType;
        target.IsLikePlaylist = source.IsLikePlaylist;
    }

    private static long ResolveUinAsync(string? cookie)
    {
        // 直接从客户端已保存的 QQ cookie 解析数字 uin（如 "uin=1548414717"）。
        // 注意不能依赖 /user/getCookie——它返回的是服务端配置文件里的 cookie（用户未配置则为空）。
        if (string.IsNullOrWhiteSpace(cookie))
            return 0;

        foreach (System.Text.RegularExpressions.Match m in
                 System.Text.RegularExpressions.Regex.Matches(cookie, @"(?:^|[; ])uin=(\d+)"))
        {
            if (long.TryParse(m.Groups[1].Value, out var uin) && uin > 0)
                return uin;
        }

        return 0;
    }

    [RelayCommand]
    private async Task OpenPlaylist(PlaylistItem item)
    {
        if (_isDisposed)
            return;

        if (item.Type == PlaylistType.AddButton) return;

        var loadVersion = Interlocked.Increment(ref _playlistLoadVersion);

        SelectedPlaylist = item;
        IsShowingSongs = true;
        _selectedPlaylistSongsDefaultOrder.Clear();
        _onlineOriginalOrder = null;
        SelectedPlaylistSongs.Clear();
        IsLoadingMore = false;

        switch (item.Type)
        {
            case PlaylistType.Local:
                await LoadLocalPlaylistAsync(item, loadVersion);
                break;
            case PlaylistType.Online when item.IsLikePlaylist:
                await LoadLikedPlaylistAsync(loadVersion);
                break;
            case PlaylistType.Online:
                await LoadOnlinePlaylistAsync(item, loadVersion);
                break;
        }
    }

    [RelayCommand]
    private Task LoadMore()
    {
        // QQ 歌单详情一次性返回全部歌曲，无分页。
        return Task.CompletedTask;
    }

    private async Task LoadLocalPlaylistAsync(PlaylistItem playlist, int loadVersion)
    {
        if (_isDisposed || playlist.ListId <= 0)
            return;

        IsLoadingMore = true;
        try
        {
            var songs = await _localPlaylistService.GetSongsAsync(playlist.ListId);
            if (_isDisposed || !IsCurrentPlaylistLoad(playlist, loadVersion))
                return;

            var songItems = new List<SongItem>();
            foreach (var song in songs)
                songItems.Add(QqLocalPlaylistService.ToSongItem(song));
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!IsCurrentPlaylistLoad(playlist, loadVersion))
                    return;

                _selectedPlaylistSongsDefaultOrder.AddRange(songItems);
                ApplySongSort();
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "加载本地歌单失败。playlistId={Id}", playlist.ListId);
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    private async Task LoadLikedPlaylistAsync(int loadVersion)
    {
        if (_isDisposed)
            return;

        IsLoadingMore = true;
        try
        {
            var cookie = _qqCookieService.Cookie;
            var uin = ResolveUinAsync(cookie);
            if (uin <= 0)
                return;

            // "我喜欢的音乐"歌单 disstid 从 QQ 官方个人信息主页解析（上游 getUserLikedSongs 依赖服务端配置 cookie，客户端不可用）
            _likedPlaylistId ??= await _userClient.GetLikedPlaylistIdAsync(uin, cookie);
            if (_likedPlaylistId is not > 0)
            {
                _logger.LogWarning("无法解析\"我喜欢的音乐\"歌单 disstid。uin={Uin}", uin);
                ShowToast("我喜欢的音乐", "无法获取喜欢歌单，请重新登录后再试", NotificationType.Warning);
                return;
            }

            var detail = await _playlistClient.GetSongListDetailAsync(_likedPlaylistId.Value.ToString(), cookie);
            if (_isDisposed || !IsCurrentPlaylistLoad(SelectedPlaylist!, loadVersion))
                return;

            var songs = detail?.SongList;
            if (songs is not { Count: > 0 })
            {
                _logger.LogInformation("我喜欢的音乐歌单无歌曲。disstid={Disstid}", _likedPlaylistId);
                return;
            }

            var songItems = songs.AsValueEnumerable().Select(MapQqSong).ToList();
            songItems = await ApplyOnlineOverridesAsync(songItems);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!IsCurrentPlaylistLoad(SelectedPlaylist!, loadVersion))
                    return;

                _selectedPlaylistSongsDefaultOrder.AddRange(songItems);
                ApplySongSort();
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "加载我喜欢的音乐失败");
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    private async Task LoadOnlinePlaylistAsync(PlaylistItem playlist, int loadVersion)
    {
        if (_isDisposed || string.IsNullOrWhiteSpace(playlist.Id))
            return;

        IsLoadingMore = true;
        try
        {
            var detail = await _playlistClient.GetSongListDetailAsync(playlist.Id, _qqCookieService.Cookie);
            if (_isDisposed || !IsCurrentPlaylistLoad(playlist, loadVersion))
                return;

            var songs = detail?.SongList;
            if (songs is not { Count: > 0 })
            {
                _logger.LogInformation("QQ 歌单详情无歌曲。disstid={Disstid}", playlist.Id);
                return;
            }

            var songItems = songs.AsValueEnumerable().Select(MapQqSong).ToList();
            songItems = await ApplyOnlineOverridesAsync(songItems);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!IsCurrentPlaylistLoad(playlist, loadVersion))
                    return;

                _selectedPlaylistSongsDefaultOrder.AddRange(songItems);
                ApplySongSort();
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "加载 QQ 歌单详情失败。disstid={Disstid}", playlist.Id);
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    private bool IsCurrentPlaylistLoad(PlaylistItem playlist, int loadVersion)
    {
        return !_isDisposed &&
               IsShowingSongs &&
               _playlistLoadVersion == loadVersion &&
               SelectedPlaylist?.Type == playlist.Type &&
               SelectedPlaylist.Id == playlist.Id &&
               SelectedPlaylist.ListId == playlist.ListId;
    }

    [RelayCommand]
    private async Task ImportExternalPlaylist()
    {
        if (IsImportingExternalPlaylist)
            return;

        var link = await _createPlaylistDialogService.PromptTextAsync(
            "导入QQ音乐歌单",
            "请输入QQ音乐歌单分享链接（如 https://y.qq.com/n/ryqq/playlist/9564315151）",
            confirmText: "下一步");

        if (string.IsNullOrWhiteSpace(link))
            return;

        IsImportingExternalPlaylist = true;
        try
        {
            var parseResult = await _externalPlaylistImportService.ParseAndLoadAsync(link);
            if (!parseResult.Success)
            {
                ShowToast("解析失败", parseResult.ErrorMessage, NotificationType.Error);
                return;
            }

            // 仅支持 QQ 音乐来源：按歌单 ID 直接拉取完整歌曲（含 songmid，可播放），导入本地歌单。
            if (!string.Equals(parseResult.SourcePlatform, "QQ音乐", StringComparison.Ordinal))
            {
                ShowToast("暂不支持", $"已识别来源：{parseResult.SourcePlatform}，当前仅支持导入QQ音乐歌单", NotificationType.Warning);
                return;
            }

            var disstid = ExtractQqPlaylistId(link);
            if (disstid <= 0)
            {
                ShowToast("解析失败", "未在链接中解析到QQ音乐歌单ID", NotificationType.Error);
                return;
            }

            var detail = await _playlistClient.GetSongListDetailAsync(disstid.ToString(), _qqCookieService.Cookie);
            var sourceSongs = detail?.SongList;
            if (sourceSongs is not { Count: > 0 })
            {
                ShowToast("导入失败", "歌单没有可导入的歌曲", NotificationType.Error);
                return;
            }

            var playlistName = await _createPlaylistDialogService.PromptTextAsync(
                "导入到本地歌单",
                $"共 {sourceSongs.Count} 首歌曲，请输入歌单名称",
                defaultValue: string.IsNullOrWhiteSpace(parseResult.SourcePlaylistName) ? "导入歌单" : parseResult.SourcePlaylistName,
                confirmText: "导入");

            if (string.IsNullOrWhiteSpace(playlistName))
                return;

            var entity = await _localPlaylistService.CreateAsync(playlistName);
            var imported = 0;
            foreach (var sourceSong in sourceSongs)
            {
                var songItem = MapQqSong(sourceSong);
                if (string.IsNullOrWhiteSpace(songItem.Hash))
                    continue;

                if (await _localPlaylistService.AddSongAsync(entity.Id, songItem))
                    imported++;
            }

            var playlistItem = new PlaylistItem
            {
                Name = entity.Name,
                Id = entity.Id.ToString(),
                ListId = entity.Id,
                Count = imported,
                Type = PlaylistType.Local,
                Cover = DefaultCover,
                Subtitle = "本地歌单"
            };
            await Dispatcher.UIThread.InvokeAsync(() => Items.Insert(0, playlistItem));

            ShowToast(
                imported > 0 ? "导入成功" : "导入完成",
                imported > 0
                    ? $"已导入 {imported} 首歌曲到本地歌单「{entity.Name}」"
                    : "没有导入任何歌曲（歌曲可能缺少播放标识）",
                imported > 0 ? NotificationType.Success : NotificationType.Warning);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导入QQ音乐歌单时发生异常");
            ShowToast("导入失败", ex.Message, NotificationType.Error);
        }
        finally
        {
            IsImportingExternalPlaylist = false;
        }
    }

    /// <summary>
    ///     从 QQ 音乐歌单分享链接中提取歌单 ID（disstid）。
    ///     支持 https://y.qq.com/n/ryqq/playlist/9564315151、?id=9564315151 以及纯数字。
    /// </summary>
    private static long ExtractQqPlaylistId(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var full = Uri.UnescapeDataString(text);

        var byPath = Regex.Match(full, @"playlist/(\d+)", RegexOptions.IgnoreCase);
        if (byPath.Success && long.TryParse(byPath.Groups[1].Value, out var pathId))
            return pathId;

        var byQuery = Regex.Match(full, @"[?&]id=(\d+)", RegexOptions.IgnoreCase);
        if (byQuery.Success && long.TryParse(byQuery.Groups[1].Value, out var queryId))
            return queryId;

        if (long.TryParse(full.Trim(), out var bareId))
            return bareId;

        return 0;
    }

    [RelayCommand]
    private async Task CreateOnlinePlaylist(string name)
    {
        // 上游 qq-music-api 无创建歌单接口（不修改上游源码），创建的歌单保存在客户端本地。
        if (string.IsNullOrWhiteSpace(name))
            return;

        var entity = await _localPlaylistService.CreateAsync(name);
        _logger.LogInformation("已创建本地歌单。id={Id} name={Name}", entity.Id, entity.Name);

        var item = new PlaylistItem
        {
            Name = entity.Name,
            Id = entity.Id.ToString(),
            ListId = entity.Id,
            Count = 0,
            Type = PlaylistType.Local,
            Cover = DefaultCover,
            Subtitle = "本地歌单"
        };
        await Dispatcher.UIThread.InvokeAsync(() => Items.Insert(0, item));

        ShowToast("新建歌单", $"已创建本地歌单「{entity.Name}」，可在歌曲上右键「添加到歌单」加入歌曲", NotificationType.Success);
    }

    [RelayCommand]
    private async Task ShowCreatePlaylistDialog()
    {
        var name = await _createPlaylistDialogService.PromptTextAsync(
            "新建歌单",
            "请输入歌单名称（创建的歌单保存在本机，QQ 音乐接口暂不支持云端创建）",
            confirmText: "创建");

        if (string.IsNullOrWhiteSpace(name))
            return;

        await CreateOnlinePlaylist(name);
    }

    [RelayCommand]
    private async Task DeleteOnlinePlaylist(PlaylistItem? item)
    {
        if (item == null)
            return;

        if (item.Type == PlaylistType.Local)
        {
            await _localPlaylistService.DeleteAsync(item.ListId);
            await Dispatcher.UIThread.InvokeAsync(() => Items.Remove(item));

            if (SelectedPlaylist?.ListId == item.ListId)
                GoBack();

            ShowToast("本地歌单", $"已删除「{item.Name}」", NotificationType.Information);
            return;
        }

        // QQ 云端没有删除接口：对在线歌单/收藏专辑/喜欢歌单做本机隐藏，避免反复出现在侧栏。
        var hideKey = QqLocalPlaylistService.GetPlaylistHideKey(item);
        await _localPlaylistService.HidePlaylistAsync(hideKey);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Items.Remove(item);
            if (SelectedPlaylist is not null && IsSamePlaylistIdentity(SelectedPlaylist, item))
                GoBack();
        });

        var displayName = item.IsLikePlaylist ? "我喜欢的音乐" : item.Name;
        ShowToast("已从本机侧栏移除", $"「{displayName}」不会再显示，云端数据未被修改", NotificationType.Information);
    }

    [RelayCommand]
    private async Task EditLocalPlaylist(PlaylistItem? item)
    {
        if (item == null || item.Type != PlaylistType.Local)
            return;

        var newName = await _createPlaylistDialogService.PromptTextAsync(
            "重命名本地歌单",
            "请输入新的歌单名称",
            item.Name,
            "保存");

        if (string.IsNullOrWhiteSpace(newName) || newName == item.Name)
            return;

        await _localPlaylistService.RenameAsync(item.ListId, newName);
        await Dispatcher.UIThread.InvokeAsync(() => item.Name = newName);
        ShowToast("本地歌单", $"已重命名为「{newName}」", NotificationType.Success);
    }

    [RelayCommand]
    private async Task DeleteLocalPlaylist(PlaylistItem? item)
    {
        if (item == null || item.Type != PlaylistType.Local)
            return;

        await _localPlaylistService.DeleteAsync(item.ListId);
        await Dispatcher.UIThread.InvokeAsync(() => Items.Remove(item));

        if (SelectedPlaylist?.ListId == item.ListId)
            GoBack();

        ShowToast("本地歌单", $"已删除「{item.Name}」", NotificationType.Information);
    }

    [RelayCommand]
    private async Task RemoveSongFromPlaylist(SongItem? song)
    {
        if (song == null)
            return;

        if (SelectedPlaylist is { Type: PlaylistType.Local })
        {
            await _localPlaylistService.RemoveSongAsync(SelectedPlaylist.ListId, song.Hash);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _selectedPlaylistSongsDefaultOrder.Remove(song);
                SelectedPlaylistSongs.Remove(song);
            });
            ShowToast("本地歌单", $"已从「{SelectedPlaylist.Name}」移除「{song.Name}」", NotificationType.Information);
            return;
        }

        // QQ 在线歌单：上游接口只读，移除记录保存在本机覆盖层，云端歌单不变。
        if (SelectedPlaylist is { Type: PlaylistType.Online })
        {
            var playlistKey = QqLocalPlaylistService.GetPlaylistHideKey(SelectedPlaylist);
            await _localPlaylistService.RemoveOnlineSongAsync(playlistKey, song.Hash);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _selectedPlaylistSongsDefaultOrder.Remove(song);
                _onlineOriginalOrder?.Remove(song);
                SelectedPlaylistSongs.Remove(song);
            });

            var displayName = SelectedPlaylist.IsLikePlaylist ? "我喜欢的音乐" : SelectedPlaylist.Name;
            ShowToast("已移除", $"已在本机从「{displayName}」移除「{song.Name}」（云端歌单不变，重新登录或换设备不受影响）", NotificationType.Information);
            return;
        }

        ShowToast("暂不支持", "该列表暂不支持移除歌曲", NotificationType.Warning);
    }

    [RelayCommand]
    private Task MoveSongUp(SongItem? song) => MoveSongAsync(song, -1);

    [RelayCommand]
    private Task MoveSongDown(SongItem? song) => MoveSongAsync(song, 1);

    [RelayCommand]
    private Task MoveSongTop(SongItem? song) => MoveSongAsync(song, int.MinValue);

    /// <summary>
    ///     手动排序：调整当前生效顺序并持久化（本地歌单重排存储，在线歌单保存本机顺序覆盖层）。
    /// </summary>
    private async Task MoveSongAsync(SongItem? song, int offset)
    {
        if (song == null || SelectedPlaylist is not ({ Type: PlaylistType.Local } or { Type: PlaylistType.Online }))
            return;

        var list = _selectedPlaylistSongsDefaultOrder;
        var index = list.IndexOf(song);
        if (index < 0 || list.Count < 2)
            return;

        var newIndex = offset == int.MinValue ? 0 : Math.Clamp(index + offset, 0, list.Count - 1);
        if (newIndex == index)
            return;

        list.RemoveAt(index);
        list.Insert(newIndex, song);

        await PersistSongOrderAsync();

        // 手动调整后切到"手动排序"视图，让用户立即看到效果；Local 歌单的默认顺序即手动顺序。
        if (GetSortMode(CurrentSortText) != PlaylistSongSortMode.Manual)
            CurrentSortText = ManualSortText;
        else
            ApplySongSort();
    }

    [RelayCommand]
    private Task DragReorderSongs(SongDragReorderRequest? request)
    {
        if (request is not { IsValid: true } ||
            SelectedPlaylist is not ({ Type: PlaylistType.Local } or { Type: PlaylistType.Online }))
            return Task.CompletedTask;

        return MoveSongToAnchorAsync(request.Song, request.Anchor, request.InsertAfter);
    }

    /// <summary>
    ///     拖拽排序落位：把歌曲移动到锚点歌曲的上方/下方并持久化。
    /// </summary>
    private async Task MoveSongToAnchorAsync(SongItem song, SongItem anchor, bool insertAfter)
    {
        var list = _selectedPlaylistSongsDefaultOrder;
        var oldIndex = list.IndexOf(song);
        var anchorIndex = list.IndexOf(anchor);
        if (oldIndex < 0 || anchorIndex < 0)
            return;

        list.RemoveAt(oldIndex);
        anchorIndex = list.IndexOf(anchor);
        var newIndex = Math.Clamp(insertAfter ? anchorIndex + 1 : anchorIndex, 0, list.Count);
        list.Insert(newIndex, song);

        await PersistSongOrderAsync();
        if (GetSortMode(CurrentSortText) != PlaylistSongSortMode.Manual)
            CurrentSortText = ManualSortText;
        else
            ApplySongSort();
    }

    /// <summary>
    ///     把当前生效顺序持久化：本地歌单重排存储，在线歌单写入本机顺序覆盖层。
    /// </summary>
    private async Task PersistSongOrderAsync()
    {
        if (SelectedPlaylist is not ({ Type: PlaylistType.Local } or { Type: PlaylistType.Online }))
            return;

        var order = _selectedPlaylistSongsDefaultOrder.AsValueEnumerable().Select(s => s.Hash).ToList();
        if (SelectedPlaylist.Type == PlaylistType.Local)
        {
            await _localPlaylistService.ReorderSongsAsync(SelectedPlaylist.ListId, order);
        }
        else
        {
            var playlistKey = QqLocalPlaylistService.GetPlaylistHideKey(SelectedPlaylist);
            await _localPlaylistService.SaveOnlineSongOrderAsync(playlistKey, order);
        }
    }

    [RelayCommand]
    private Task BatchRemoveFromPlaylist(IList? selectedSongs)
    {
        return RemoveSongsInternalAsync(ToSongList(selectedSongs), sourceOnly: true);
    }

    [RelayCommand]
    private Task BatchCopyToPlaylist(IList? selectedSongs)
    {
        var songs = ToSongList(selectedSongs);
        if (songs.Count == 0)
            return Task.CompletedTask;

        return _favoritePlaylistService.ShowBatchCopyToPlaylistDialogAsync(songs);
    }

    [RelayCommand]
    private Task BatchMoveToPlaylist(IList? selectedSongs)
    {
        var songs = ToSongList(selectedSongs);
        if (songs.Count == 0)
            return Task.CompletedTask;

        // 复制到目标歌单成功确认后，再从当前歌单移除，构成"移动"语义。
        return _favoritePlaylistService.ShowBatchCopyToPlaylistDialogAsync(
            songs,
            afterConfirmed: () => _ = RemoveSongsInternalAsync(songs, sourceOnly: false));
    }

    private static List<SongItem> ToSongList(IList? selected)
    {
        if (selected is null || selected.Count == 0)
            return [];

        var songs = new List<SongItem>(selected.Count);
        foreach (var item in selected)
        {
            if (item is SongItem song)
                songs.Add(song);
        }

        return songs;
    }

    /// <summary>
    ///     批量移除当前歌单中的歌曲（本地歌单删存储，在线歌单写本机覆盖层）。
    /// </summary>
    private async Task RemoveSongsInternalAsync(IReadOnlyList<SongItem> songs, bool sourceOnly)
    {
        if (songs.Count == 0 || SelectedPlaylist is not ({ Type: PlaylistType.Local } or { Type: PlaylistType.Online }))
            return;

        var hashes = songs.AsValueEnumerable()
            .Where(s => !string.IsNullOrWhiteSpace(s.Hash))
            .Select(s => s.Hash)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (hashes.Count == 0)
            return;

        var playlistName = SelectedPlaylist.IsLikePlaylist ? "我喜欢的音乐" : SelectedPlaylist.Name;

        if (SelectedPlaylist.Type == PlaylistType.Local)
        {
            await _localPlaylistService.RemoveSongsAsync(SelectedPlaylist.ListId, hashes);
        }
        else
        {
            var playlistKey = QqLocalPlaylistService.GetPlaylistHideKey(SelectedPlaylist);
            await _localPlaylistService.RemoveOnlineSongsAsync(playlistKey, hashes);
        }

        var removedSet = new HashSet<string>(hashes, StringComparer.OrdinalIgnoreCase);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            for (var i = _selectedPlaylistSongsDefaultOrder.Count - 1; i >= 0; i--)
            {
                if (removedSet.Contains(_selectedPlaylistSongsDefaultOrder[i].Hash))
                    _selectedPlaylistSongsDefaultOrder.RemoveAt(i);
            }

            if (_onlineOriginalOrder is not null)
            {
                for (var i = _onlineOriginalOrder.Count - 1; i >= 0; i--)
                {
                    if (removedSet.Contains(_onlineOriginalOrder[i].Hash))
                        _onlineOriginalOrder.RemoveAt(i);
                }
            }

            for (var i = SelectedPlaylistSongs.Count - 1; i >= 0; i--)
            {
                if (removedSet.Contains(SelectedPlaylistSongs[i].Hash))
                    SelectedPlaylistSongs.RemoveAt(i);
            }
        });

        if (!sourceOnly)
        {
            ShowToast(
                "移动完成",
                $"已把 {hashes.Count} 首歌曲移出「{playlistName}」",
                NotificationType.Success);
        }
        else
        {
            ShowToast(
                "批量删除完成",
                $"已从「{playlistName}」移除 {hashes.Count} 首歌曲" +
                (SelectedPlaylist.Type == PlaylistType.Online ? "（云端歌单不变）" : ""),
                NotificationType.Success);
        }
    }

    /// <summary>
    ///     应用在线歌单本机覆盖层：过滤已移除歌曲，并按保存的手动顺序重排。
    /// </summary>
    private async Task<List<SongItem>> ApplyOnlineOverridesAsync(List<SongItem> songItems)
    {
        if (SelectedPlaylist is not { Type: PlaylistType.Online })
            return songItems;

        var playlistKey = QqLocalPlaylistService.GetPlaylistHideKey(SelectedPlaylist);
        var removed = await _localPlaylistService.GetOnlineRemovedSongsAsync(playlistKey);
        if (removed.Count > 0)
            songItems = songItems.AsValueEnumerable().Where(s => !removed.Contains(s.Hash)).ToList();

        // 保存云端原始顺序（供"默认排序"使用）。
        _onlineOriginalOrder = new List<SongItem>(songItems);

        var order = await _localPlaylistService.GetOnlineSongOrderAsync(playlistKey);
        if (order.Count == 0)
            return songItems;

        var orderIndex = new Dictionary<string, int>(order.Count, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < order.Count; i++)
            orderIndex.TryAdd(order[i], i);

        songItems.Sort((a, b) =>
        {
            var ia = orderIndex.TryGetValue(a.Hash, out var va) ? va : int.MaxValue;
            var ib = orderIndex.TryGetValue(b.Hash, out var vb) ? vb : int.MaxValue;
            return ia.CompareTo(ib);
        });
        return songItems;
    }

    /// <summary>
    ///     添加歌曲到本地歌单（供"添加到歌单"对话框调用）。
    /// </summary>
    public async Task<bool> AddSongToLocalPlaylistAsync(long playlistId, SongItem song)
    {
        if (playlistId <= 0 || song == null)
            return false;

        var added = await _localPlaylistService.AddSongAsync(playlistId, song);
        if (added)
            ShowToast("本地歌单", $"已把「{song.Name}」加入歌单", NotificationType.Success);
        else
            ShowToast("本地歌单", "这首歌已在歌单中", NotificationType.Information);
        return added;
    }

    private void ApplySongSort()
    {
        IEnumerable<SongItem> sortedSongs = GetSortMode(CurrentSortText) switch
        {
            PlaylistSongSortMode.Artist => _selectedPlaylistSongsDefaultOrder
                .AsValueEnumerable().OrderBy(song => song.Singer, StringComparer.CurrentCultureIgnoreCase).ToArray(),
            PlaylistSongSortMode.Album => _selectedPlaylistSongsDefaultOrder
                .AsValueEnumerable().OrderBy(song => song.AlbumName, StringComparer.CurrentCultureIgnoreCase).ToArray(),
            // 手动排序：使用当前生效顺序（在线歌单已按本机保存的手动顺序重排）。
            PlaylistSongSortMode.Manual => _selectedPlaylistSongsDefaultOrder,
            // 默认排序：在线歌单展示云端原始顺序（仍应用本机移除过滤）。
            PlaylistSongSortMode.Default when _onlineOriginalOrder is not null => _onlineOriginalOrder,
            _ => _selectedPlaylistSongsDefaultOrder
        };

        SelectedPlaylistSongs.Clear();
        SelectedPlaylistSongs.AddRange(sortedSongs);
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

    private void ShowToast(string title, string content, NotificationType type)
    {
        _toastManager.CreateToast()
            .OfType(type)
            .WithTitle(title)
            .WithContent(content)
            .Dismiss().After(TimeSpan.FromSeconds(4))
            .Dismiss().ByClicking()
            .Queue();
    }

    private static PlaylistSongSortMode GetSortMode(string? value)
    {
        return value switch
        {
            ArtistSortText => PlaylistSongSortMode.Artist,
            AlbumSortText => PlaylistSongSortMode.Album,
            ManualSortText => PlaylistSongSortMode.Manual,
            _ => PlaylistSongSortMode.Default
        };
    }

    private static string GetSortText(PlaylistSongSortMode mode)
    {
        return mode switch
        {
            PlaylistSongSortMode.Artist => ArtistSortText,
            PlaylistSongSortMode.Album => AlbumSortText,
            PlaylistSongSortMode.Manual => ManualSortText,
            _ => DefaultSortText
        };
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _messenger.UnregisterAll(this);
    }
}
