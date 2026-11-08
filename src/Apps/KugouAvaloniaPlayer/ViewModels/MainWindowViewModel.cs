using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using ZLinq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KuGou.Net.Protocol.Session;
using KugouAvaloniaPlayer.Models;
using KugouAvaloniaPlayer.Services;
using Microsoft.Extensions.Logging;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace KugouAvaloniaPlayer.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private static readonly Lock CustomBackgroundImageSync = new();
    private static Bitmap? _sCachedCustomBackgroundBitmap;
    private static string? _sCachedCustomBackgroundPath;
    private static int _sCachedCustomBackgroundDecodeWidth;

    private const int DefaultCustomBackgroundDecodeWidth = 1920;
    private const int MaxCustomBackgroundDecodeWidth = 1920;

    private readonly IAppUpdateService _appUpdateService;
    private readonly IDesktopLyricWindowService _desktopLyricWindowService;
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly ILoginDialogService _loginDialogService;
    private readonly ILoginInitializationService _loginInitializationService;
    private readonly IMainWindowService _mainWindowService;
    private readonly IMessenger _messenger;
    private readonly INavigationService _navigationService;
    private readonly IUiPreferencesState _uiPreferencesState;
    private readonly IVipEntitlementService _vipEntitlementService;
    private readonly SearchViewModel _searchViewModel;
    private readonly DiscoverViewModel _discoverViewModel;
    private readonly KgSessionManager _sessionManager;
    private readonly SettingViewModel _settingViewModel;

    [ObservableProperty]
    public partial PageViewModelBase ActivePage { get; set; }

    [ObservableProperty]
    public partial PageViewModelBase? SelectedMenuPage { get; set; }

    [ObservableProperty]
    public partial bool IsDesktopLyricEnabled { get; set; }

    [ObservableProperty]
    public partial bool IsLoggedIn { get; set; }

    [ObservableProperty]
    public partial bool IsCreatedPlaylistsExpanded { get; set; } = true;

    [ObservableProperty]
    public partial bool IsCollectedPlaylistsExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsAlbumPlaylistsExpanded { get; set; }

    [ObservableProperty]
    public partial PlaylistItem? QuickLikePlaylist { get; set; }

    [ObservableProperty]
    public partial bool IsMiniPlayerOpaque { get; set; } = true;

    private bool _isUpdatingActivePageFromNavigation;
    private bool _isUpdatingSelectedMenuPageFromNavigation;
    private bool _isClosingDesktopLyricForShutdown;
    private bool _sidebarRefreshPending;
    private bool _isDisposed;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    public partial string SearchKeyword { get; set; } = "";

    [ObservableProperty]
    public partial string? UserAvatar { get; set; }

    [ObservableProperty]
    public partial string UserName { get; set; } = "未登录";

    public MainWindowViewModel(
        ISukiToastManager toastManager,
        PlayerViewModel player,
        ISukiDialogManager dialogManager,
        KgSessionManager sessionManager,
        ISongInteractionService songInteractionService,
        IAppUpdateService appUpdateService,
        IDesktopLyricWindowService desktopLyricWindowService,
        ILoginDialogService loginDialogService,
        ILoginInitializationService loginInitializationService,
        IMainWindowService mainWindowService,
        IMessenger messenger,
        IUiPreferencesState uiPreferencesState,
        IVipEntitlementService vipEntitlementService,
        INavigationService navigationService,
        NowPlayingViewModel nowPlaying,
        LoginViewModel loginViewModel,
        SearchViewModel searchViewModel,
        SettingViewModel settingViewModel,
        HistoryViewModel historyViewModel,
        DiscoverViewModel discoverViewModel,
        LocalMusicLibraryViewModel localMusicLibraryViewModel,
        MyPlaylistsViewModel myPlaylistsViewModel,
        ILogger<MainWindowViewModel> logger)
    {
        DialogManager = dialogManager;
        _sessionManager = sessionManager;
        _appUpdateService = appUpdateService;
        _desktopLyricWindowService = desktopLyricWindowService;
        _loginDialogService = loginDialogService;
        _loginInitializationService = loginInitializationService;
        _mainWindowService = mainWindowService;
        _messenger = messenger;
        _uiPreferencesState = uiPreferencesState;
        _vipEntitlementService = vipEntitlementService;
        _navigationService = navigationService;

        LoginViewModel = loginViewModel;
        _searchViewModel = searchViewModel;
        _discoverViewModel = discoverViewModel;
        _settingViewModel = settingViewModel;
        PlaylistsViewModel = myPlaylistsViewModel;
        _logger = logger;

        _settingViewModel.CheckForUpdateRequested += OnCheckForUpdateRequested;
        _desktopLyricWindowService.IsOpenChanged += OnDesktopLyricWindowStateChanged;

        Player = player;
        SongInteractions = songInteractionService;
        NowPlaying = nowPlaying;
        ToastManager = toastManager;

        Pages.Add(historyViewModel);
        Pages.Add(localMusicLibraryViewModel);
        Pages.Add(discoverViewModel);
        _navigationService.CurrentPageChanged += OnNavigationCurrentPageChanged;
        _navigationService.NavigateRoot(discoverViewModel);
        ActivePage = discoverViewModel;
        SelectedMenuPage = discoverViewModel;
        IsDesktopLyricEnabled = _desktopLyricWindowService.IsOpen;
        ApplyCustomBackgroundImage(_uiPreferencesState.Current.AppBackground);
        _uiPreferencesState.PropertyChanged += OnUiPreferencesChanged;

        PlaylistsViewModel.Items.CollectionChanged += OnPlaylistItemsChanged;
        PlaylistsViewModel.PropertyChanged += OnPlaylistViewModelPropertyChanged;
        RefreshSidebarPlaylists();

        _messenger.Register<AuthStateChangedEvent>(this, (_, m) =>
        {
            if (m.IsLoggedIn)
                _ = OnLoginSuccessAsync();
            else
                OnLogoutRequested();
        });

        _ = InitializeStartupAsync();

        _ = ApplyDeferredStartupPreferencesAsync();
    }

    private async Task InitializeStartupAsync()
    {
        try
        {
            await LoadLocalSessionOrLogin();
            if (SettingsManager.Settings.AutoCheckUpdate)
                await _appUpdateService.CheckForUpdatesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动后台初始化任务失败");
        }
    }

    private async Task ApplyDeferredStartupPreferencesAsync()
    {
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(800));
            await Player.RestoreCachedPlaybackQueueAsync();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var savedPlaybackMode = SettingsManager.Settings.PlaybackMode;
                if (savedPlaybackMode != PlayMode.Normal)
                    Player.ApplySavedPlaybackModePreference();

                if (SettingsManager.Settings.OpenDesktopLyricOnStartup && !_desktopLyricWindowService.IsOpen)
                    _desktopLyricWindowService.Toggle();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "应用延迟启动设置失败");
        }
    }

    private void ApplyCustomBackgroundImage(
        bool useCustomImage,
        string? customImagePath,
        double customImageOpacity)
    {
        var hasCustomImage = UpdateCustomBackgroundBrush(useCustomImage, customImagePath, Math.Clamp(customImageOpacity, 0.1, 1.0));
        IsMiniPlayerOpaque = !hasCustomImage;
    }

    private static bool UpdateCustomBackgroundBrush(bool useCustomImage, string? path, double opacity)
    {
        // 视频壁纸：走 LibVLC 渲染层，不再生成图片画刷（启动兜底再校验一次 20MB 上限）
        if (useCustomImage &&
            !string.IsNullOrWhiteSpace(path) &&
            System.IO.File.Exists(path) &&
            VideoWallpaperService.IsVideoWallpaperFile(path) &&
            new System.IO.FileInfo(path).Length <= VideoWallpaperService.MaxVideoWallpaperBytes)
        {
            VideoWallpaperService.Play(path, Math.Clamp(opacity, 0.1, 1.0));
            Dispatcher.UIThread.Post(() =>
            {
                if (Avalonia.Application.Current is not { } app) return;
                app.Resources["KugouCustomBackgroundBrush"] = Brushes.Transparent;
                app.Resources["KugouHeroBackgroundImageVisible"] = false;
            });
            return true;
        }

        // 图片 / 关闭路径：先停掉视频壁纸再走原有图片逻辑
        VideoWallpaperService.Stop();

        var brush = CreateCustomBackgroundBrush(useCustomImage, path, opacity, out var hasCustomImage);
        Dispatcher.UIThread.Post(() =>
        {
            if (Avalonia.Application.Current is not { } app) return;
            app.Resources["KugouCustomBackgroundBrush"] = brush;
            app.Resources["KugouHeroBackgroundImageVisible"] = !hasCustomImage;
        });

        return hasCustomImage;
    }

    private static IBrush CreateCustomBackgroundBrush(
        bool useCustomImage,
        string? path,
        double opacity,
        out bool hasCustomImage)
    {
        hasCustomImage = false;

        if (!useCustomImage || string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
        {
            ReleaseCachedCustomBackgroundBitmap();
            return Brushes.Transparent;
        }

        try
        {
            var brush = new ImageBrush(GetOrCreateCustomBackgroundBitmap(path))
            {
                Stretch = Stretch.UniformToFill,
                Opacity = opacity
            };
            hasCustomImage = true;
            return brush;
        }
        catch
        {
            ReleaseCachedCustomBackgroundBitmap(path);
            return Brushes.Transparent;
        }
    }

    private static Bitmap GetOrCreateCustomBackgroundBitmap(string path)
    {
        var decodeWidth = GetCustomBackgroundDecodeWidth();

        lock (CustomBackgroundImageSync)
        {
            if (_sCachedCustomBackgroundBitmap is not null &&
                string.Equals(_sCachedCustomBackgroundPath, path, StringComparison.OrdinalIgnoreCase) &&
                _sCachedCustomBackgroundDecodeWidth == decodeWidth)
            {
                return _sCachedCustomBackgroundBitmap;
            }

            _sCachedCustomBackgroundBitmap?.Dispose();
            using var stream = File.OpenRead(path);
            _sCachedCustomBackgroundBitmap = Bitmap.DecodeToWidth(stream, decodeWidth);
            _sCachedCustomBackgroundPath = path;
            _sCachedCustomBackgroundDecodeWidth = decodeWidth;
            return _sCachedCustomBackgroundBitmap;
        }
    }

    private static int GetCustomBackgroundDecodeWidth()
    {
        var mainWindow = (Avalonia.Application.Current?.ApplicationLifetime
                           as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)
            ?.MainWindow;

        var renderScaling = mainWindow?.RenderScaling ?? 1;
        var clientWidth = mainWindow?.ClientSize.Width ?? 0;
        var width = clientWidth > 0
            ? (int)Math.Ceiling(clientWidth * renderScaling)
            : DefaultCustomBackgroundDecodeWidth;

        return Math.Clamp(width, 1, MaxCustomBackgroundDecodeWidth);
    }

    private static void ReleaseCachedCustomBackgroundBitmap(string? path = null)
    {
        lock (CustomBackgroundImageSync)
        {
            if (path is not null &&
                !string.Equals(_sCachedCustomBackgroundPath, path, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _sCachedCustomBackgroundBitmap?.Dispose();
            _sCachedCustomBackgroundBitmap = null;
            _sCachedCustomBackgroundPath = null;
            _sCachedCustomBackgroundDecodeWidth = 0;
        }
    }

    //public string Version => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    public PlayerViewModel Player { get; }
    public ISongInteractionService SongInteractions { get; }
    public NowPlayingViewModel NowPlaying { get; }
    public ISukiToastManager ToastManager { get; }
    public ISukiDialogManager DialogManager { get; }

    public AvaloniaList<PageViewModelBase> Pages { get; } = new();

    private LoginViewModel LoginViewModel { get; }
    public MyPlaylistsViewModel PlaylistsViewModel { get; }

    public AvaloniaList<PlaylistItem> SidebarCreatedPlaylists { get; } = new();
    public AvaloniaList<PlaylistItem> SidebarCollectedPlaylists { get; } = new();
    public AvaloniaList<PlaylistItem> SidebarAlbumPlaylists { get; } = new();

    private void OnDesktopLyricWindowStateChanged(bool isOpen)
    {
        if (!_isClosingDesktopLyricForShutdown)
        {
            SettingsManager.Settings.OpenDesktopLyricOnStartup = isOpen;
            SettingsManager.Save();
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            IsDesktopLyricEnabled = isOpen;
            return;
        }

        Dispatcher.UIThread.Post(() => IsDesktopLyricEnabled = isOpen);
    }

    partial void OnIsCreatedPlaylistsExpandedChanged(bool value)
    {
        if (!value)
            return;

        IsCollectedPlaylistsExpanded = false;
        IsAlbumPlaylistsExpanded = false;
    }

    partial void OnIsCollectedPlaylistsExpandedChanged(bool value)
    {
        if (!value)
            return;

        IsCreatedPlaylistsExpanded = false;
        IsAlbumPlaylistsExpanded = false;
    }

    partial void OnIsAlbumPlaylistsExpandedChanged(bool value)
    {
        if (!value)
            return;

        IsCreatedPlaylistsExpanded = false;
        IsCollectedPlaylistsExpanded = false;
    }

    partial void OnActivePageChanged(PageViewModelBase value)
    {
        if (_isUpdatingActivePageFromNavigation)
            return;

        NavigateToPage(value);
    }

    partial void OnSelectedMenuPageChanged(PageViewModelBase? value)
    {
        if (_isUpdatingSelectedMenuPageFromNavigation)
            return;

        // 侧栏歌单分组的高亮只跟随"我的歌单"页；切到其他固定页时取消分组高亮，
        // 避免出现"探索发现/播放历史 + 某个歌单"同时高亮的二义性状态。
        UpdateSidebarSelection();

        if (value == null)
            return;

        NavigateToPage(value);
    }

    /// <summary>
    ///     点击侧栏"已选中"的固定页时触发（TreeView 点击已选中项不产生属性变更，
    ///     由 KugouSideMenu 显式回调）。用于从 transient 详情页一键回到该页根视图。
    /// </summary>
    [RelayCommand]
    private void ReopenSelectedPage(PageViewModelBase? page)
    {
        if (page is null || !Pages.Contains(page))
            return;

        if (_navigationService.CurrentPage != page)
        {
            NavigateToPage(page);
            return;
        }

        // 已在该固定页，但上面可能叠加了 transient 详情：重置回页根。
        _navigationService.NavigateRoot(page);
    }

    private void OnNavigationCurrentPageChanged(PageViewModelBase? page)
    {
        if (page == null)
            return;

        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplyNavigationPage(page);
            return;
        }

        Dispatcher.UIThread.Post(() => ApplyNavigationPage(page));
    }

    private void ApplyNavigationPage(PageViewModelBase page)
    {
        NowPlaying.CloseCommand.Execute(null);
        _isUpdatingActivePageFromNavigation = true;
        ActivePage = page;
        _isUpdatingActivePageFromNavigation = false;

        if (!Pages.Contains(page))
            return;

        _isUpdatingSelectedMenuPageFromNavigation = true;
        SelectedMenuPage = page;
        _isUpdatingSelectedMenuPageFromNavigation = false;
    }

    private void NavigateToPage(PageViewModelBase page)
    {
        if (_navigationService.CurrentPage == page)
            return;

        if (Pages.Contains(page))
        {
            _navigationService.NavigateRoot(page);
            return;
        }

        _navigationService.Navigate(page);
    }

    // --- 登录相关 ---
    private async Task LoadLocalSessionOrLogin()
    {
        var result = await _loginInitializationService.InitializeLocalSessionAsync();
        if (!result.IsLoggedIn)
            return;

        IsLoggedIn = true;
        ApplyUserProfile(result.Profile);

        if (result.UserProfileLoadFailed)
            ShowLoadUserInfoFailedToast();

        _ = InitializeVipAndLikesAsync("获取VIP或喜欢列表失败", LogLevel.Warning);
    }

    private async Task LoadUserInfo()
    {
        var result = await _loginInitializationService.LoadCurrentUserProfileAsync();
        ApplyUserProfile(result.Profile);
        if (result.Failed)
            ShowLoadUserInfoFailedToast();
    }

    private async Task OnLoginSuccessAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            DialogManager.DismissDialog();
            IsLoggedIn = true;
        });

        await LoadUserInfo();
        await PlaylistsViewModel.ReloadAllPlaylistsAsync();
        _logger.LogInformation("登录成功");

        _ = InitializeVipAndLikesAsync("初始化VIP或喜欢列表失败", LogLevel.Error);
    }

    private async Task InitializeVipAndLikesAsync(string failureMessage, LogLevel logLevel)
    {
        try
        {
            // QQ 音乐模式下无 VIP 领取能力，查询失败静默处理（仅记日志，不弹"查询VIP失败"）。
            var vipResult = await _vipEntitlementService.TryEnsureDailyVipAsync();
            if (!vipResult.Success)
                _logger.Log(logLevel, "VIP 状态查询未成功（QQ 模式无 VIP 权益，静默）：{Reason}", vipResult.FailureReason);

            await Player.LoadLikeListAsync();
        }
        catch (Exception ex)
        {
            _logger.Log(logLevel, ex, failureMessage);
        }
    }

    private void ApplyUserProfile(UserProfileSnapshot? profile)
    {
        if (profile == null)
            return;

        UserName = profile.UserName;
        UserAvatar = profile.UserAvatar;
        _settingViewModel.UserName = profile.UserName;
        _settingViewModel.UserAvatar = profile.UserAvatar;
        _settingViewModel.UserId = profile.UserId;
    }

    private void ShowLoadUserInfoFailedToast()
    {
        ToastManager.CreateToast()
            .OfType(NotificationType.Warning)
            .WithTitle("加载用户失败")
            .Dismiss().After(TimeSpan.FromSeconds(3))
            .Dismiss().ByClicking()
            .Queue();
    }

    private void OnLogoutRequested()
    {
        Dispatcher.UIThread.Post(() =>
        {
            IsLoggedIn = false;
            UserName = "未登录";
            UserAvatar = null;
            _settingViewModel.UserName = UserName;
            _settingViewModel.UserAvatar = null;
            _settingViewModel.UserId = string.Empty;
            _settingViewModel.VipStatus = "未开通";
            Player.ClearPersonalFmSession();
            _ = PlaylistsViewModel.ReloadAllPlaylistsAsync();
            _logger.LogInformation("已退出登录");

            // 返回每日推荐页面
            _navigationService.NavigateRoot(_discoverViewModel);
        });
    }

    private void OnCheckForUpdateRequested()
    {
        _ = CheckForUpdatesFromUserAsync();
    }

    private async Task CheckForUpdatesFromUserAsync()
    {
        try
        {
            await _appUpdateService.CheckForUpdatesAsync(true);
        }
        finally
        {
            Dispatcher.UIThread.Post(() => _settingViewModel.SetCheckingUpdateState(false));
        }
    }

    [RelayCommand]
    private void ShowLoginDialog()
    {
        _loginDialogService.ShowLoginDialog(LoginViewModel);
    }

    [RelayCommand]
    private void NavigateToUser()
    {
        if (IsLoggedIn)
            _ = _settingViewModel.LoadUserInfoAsync();

        NavigateToPage(_settingViewModel);
    }

    [RelayCommand]
    private void RequestWindowToggleFullScreen()
    {
        _mainWindowService.ToggleFullScreen();
    }

    [RelayCommand]
    private void RequestWindowMinimize()
    {
        _mainWindowService.Minimize();
    }

    [RelayCommand]
    private void RequestWindowToggleMaximize()
    {
        _mainWindowService.ToggleMaximize();
    }

    [RelayCommand]
    private void RequestWindowClose()
    {
        _mainWindowService.Close();
    }

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchKeyword))
            return;

        _navigationService.NavigateTransient(_searchViewModel);
        await _searchViewModel.SearchAsync(SearchKeyword);
    }

    private bool CanSearch()
    {
        return !string.IsNullOrWhiteSpace(SearchKeyword);
    }

    [RelayCommand]
    private async Task OpenSidebarPlaylist(PlaylistItem? item)
    {
        if (item == null || item.Type == PlaylistType.AddButton) return;

        NavigateToPage(PlaylistsViewModel);
        await PlaylistsViewModel.OpenPlaylistCommand.ExecuteAsync(item);
    }

    private void OnPlaylistViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MyPlaylistsViewModel.SelectedPlaylist))
            UpdateSidebarSelection();
    }

    private void OnPlaylistItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isDisposed)
            return;

        if (_sidebarRefreshPending)
            return;

        _sidebarRefreshPending = true;
        Dispatcher.UIThread.Post(() =>
        {
            _sidebarRefreshPending = false;
            if (!_isDisposed)
                RefreshSidebarPlaylists();
        }, DispatcherPriority.Background);
    }

    private void UpdateSidebarSelection()
    {
        // 歌单分组高亮只在"我的歌单"页有效，其他页面（探索发现/播放历史等）
        // 打开详情时不能残留歌单项高亮，否则侧栏出现两组选中。
        var selected = ReferenceEquals(SelectedMenuPage, PlaylistsViewModel)
            ? PlaylistsViewModel.SelectedPlaylist
            : null;

        foreach (var item in SidebarCreatedPlaylists)
            item.IsSelected = IsSameItem(selected, item);
        foreach (var item in SidebarCollectedPlaylists)
            item.IsSelected = IsSameItem(selected, item);
        foreach (var item in SidebarAlbumPlaylists)
            item.IsSelected = IsSameItem(selected, item);
    }

    private static bool IsSameItem(PlaylistItem? a, PlaylistItem b)
    {
        if (a == null) return false;
        if (a.Type != b.Type) return false;

        return a.Type switch
        {
            PlaylistType.Online or PlaylistType.Album => a.ListId > 0 && b.ListId > 0
                ? a.ListId == b.ListId
                : string.Equals(a.Id, b.Id, StringComparison.OrdinalIgnoreCase),
            PlaylistType.Local => a.ListId > 0 && a.ListId == b.ListId,
            _ => false
        };
    }

    private void RefreshSidebarPlaylists()
    {
        SyncSidebarCollection(
            SidebarCreatedPlaylists,
            PlaylistsViewModel.Items.AsValueEnumerable().Where(IsCreatedOnlinePlaylist).ToArray());
        SyncSidebarCollection(
            SidebarCollectedPlaylists,
            PlaylistsViewModel.Items.AsValueEnumerable().Where(IsCollectedOnlinePlaylist).ToArray());
        SyncSidebarCollection(
            SidebarAlbumPlaylists,
            PlaylistsViewModel.Items.AsValueEnumerable().Where(x => x.Type == PlaylistType.Album).ToArray());
        QuickLikePlaylist = PlaylistsViewModel.Items.AsValueEnumerable().FirstOrDefault(x => x.IsLikePlaylist);

        UpdateSidebarSelection();
    }

    private static void SyncSidebarCollection(
        AvaloniaList<PlaylistItem> target,
        IReadOnlyList<PlaylistItem> source)
    {
        for (var i = target.Count - 1; i >= 0; i--)
        {
            if (!ContainsReference(source, target[i]))
                target.RemoveAt(i);
        }

        for (var i = 0; i < source.Count; i++)
        {
            var desired = source[i];
            var existingIndex = IndexOfReference(target, desired);
            if (existingIndex < 0)
            {
                target.Insert(i, desired);
                continue;
            }

            if (existingIndex != i)
                target.Move(existingIndex, i);
        }

        while (target.Count > source.Count)
            target.RemoveAt(target.Count - 1);
    }

    private static bool ContainsReference(IReadOnlyList<PlaylistItem> items, PlaylistItem target)
    {
        return IndexOfReference(items, target) >= 0;
    }

    private static int IndexOfReference(IReadOnlyList<PlaylistItem> items, PlaylistItem target)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (ReferenceEquals(items[i], target))
                return i;
        }

        return -1;
    }

    private static bool IsCreatedOnlinePlaylist(PlaylistItem item)
    {
        // 创建的歌单 = 在线歌单（QQ 云端）+ 本地歌单（导入/新建，Type=Local）
        return item.Type == PlaylistType.Local
               || (item.Type == PlaylistType.Online && item.UserPlaylistType == 0);
    }

    private static bool IsCollectedOnlinePlaylist(PlaylistItem item)
    {
        return item.Type == PlaylistType.Online && item.UserPlaylistType != 0;
    }

    [RelayCommand]
    private void ToggleDesktopLyric()
    {
        SettingsManager.Settings.OpenDesktopLyricOnStartup = !_desktopLyricWindowService.IsOpen;
        SettingsManager.Save();
        _desktopLyricWindowService.Toggle();
    }


    [RelayCommand]
    private void NavigateBack()
    {
        if (_navigationService.GoBack())
            return;

        if (Pages.Contains(_discoverViewModel))
            _navigationService.NavigateRoot(_discoverViewModel);
    }


    public void ForceCloseDesktopLyric()
    {
        _isClosingDesktopLyricForShutdown = true;
        _desktopLyricWindowService.Close();
    }

    private void OnUiPreferencesChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IUiPreferencesState.Current))
            ApplyCustomBackgroundImage(_uiPreferencesState.Current.AppBackground);
    }

    private void ApplyCustomBackgroundImage(AppBackgroundPreferences preferences)
    {
        ApplyCustomBackgroundImage(
            preferences.UseCustomImage,
            preferences.CustomImagePath,
            preferences.CustomImageOpacity);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _messenger.UnregisterAll(this);
        _uiPreferencesState.PropertyChanged -= OnUiPreferencesChanged;
        _settingViewModel.CheckForUpdateRequested -= OnCheckForUpdateRequested;
        _desktopLyricWindowService.IsOpenChanged -= OnDesktopLyricWindowStateChanged;
        _navigationService.CurrentPageChanged -= OnNavigationCurrentPageChanged;
        PlaylistsViewModel.Items.CollectionChanged -= OnPlaylistItemsChanged;
        PlaylistsViewModel.PropertyChanged -= OnPlaylistViewModelPropertyChanged;
    }

}
