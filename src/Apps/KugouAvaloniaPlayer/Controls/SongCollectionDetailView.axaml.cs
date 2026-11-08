using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using ZLinq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using KuGou.Net.Abstractions.Models;
using KugouAvaloniaPlayer.Models;
using KugouAvaloniaPlayer.Services;
using KugouAvaloniaPlayer.ViewModels;
using SukiUI;

namespace KugouAvaloniaPlayer.Controls;

public partial class SongCollectionDetailView : UserControl
{
    private INotifyCollectionChanged? _songsCollectionNotifier;
    private PlayerViewModel? _subscribedPlayer;
    private readonly DispatcherTimer _songSearchDebounceTimer;
    private long _lastLocatedRequestSequence;

    public static readonly StyledProperty<string?> CoverProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string?>(nameof(Cover));

    public static readonly StyledProperty<bool> ShowCoverProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(ShowCover), true);

    public static readonly StyledProperty<bool> ShowSongListProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(ShowSongList), true);

    public static readonly StyledProperty<bool> ShowPlayAllButtonProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(ShowPlayAllButton), true);

    public static readonly StyledProperty<string> HeroBackgroundProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string>(nameof(HeroBackground), string.Empty);

    public static readonly StyledProperty<string> LightHeroBackgroundProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string>(
            nameof(LightHeroBackground),
            "avares://QqMusicPlayer/Assets/light.png");

    public static readonly StyledProperty<string> NightHeroBackgroundProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string>(
            nameof(NightHeroBackground),
            "avares://QqMusicPlayer/Assets/night.png");

    public static readonly StyledProperty<string> CurrentHeroBackgroundProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string>(
            nameof(CurrentHeroBackground),
            "avares://QqMusicPlayer/Assets/light.png");

    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string?>(nameof(Title));

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string?>(nameof(Subtitle));

    public static readonly StyledProperty<bool> HasSubtitleProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(HasSubtitle));

    public static readonly StyledProperty<IEnumerable?> SongsProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, IEnumerable?>(nameof(Songs));

    public static readonly StyledProperty<bool> EnableSongSearchProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(EnableSongSearch));

    public static readonly StyledProperty<string?> SongSearchTextProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string?>(nameof(SongSearchText));

    public static readonly StyledProperty<SongLocateRequest?> SongLocateRequestProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, SongLocateRequest?>(nameof(SongLocateRequest));

    public static readonly StyledProperty<ICommand?> LoadMoreCommandProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, ICommand?>(nameof(LoadMoreCommand));

    public static readonly StyledProperty<ICommand?> BackCommandProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, ICommand?>(nameof(BackCommand));

    public static readonly StyledProperty<bool> ShowBackButtonProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(ShowBackButton), true);

    public static readonly StyledProperty<ICommand?> PlayFirstCommandProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, ICommand?>(nameof(PlayFirstCommand));

    public static readonly StyledProperty<IPlaybackCommands?> PlaybackCommandsProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, IPlaybackCommands?>(nameof(PlaybackCommands));

    public static readonly StyledProperty<ISongInteractionService?> SongInteractionsProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, ISongInteractionService?>(nameof(SongInteractions));

    public static readonly StyledProperty<ICommand?> RemoveSongCommandProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, ICommand?>(nameof(RemoveSongCommand));

    public static readonly StyledProperty<ICommand?> SetLocalSongCoverCommandProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, ICommand?>(nameof(SetLocalSongCoverCommand));

    public static readonly StyledProperty<ICommand?> SearchLocalPlaylistsCommandProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, ICommand?>(nameof(SearchLocalPlaylistsCommand));

    public static readonly StyledProperty<bool> HasPlayFirstCommandProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(HasPlayFirstCommand));

    public static readonly StyledProperty<bool> ShowAddLoadedSongsToQueueButtonProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(ShowAddLoadedSongsToQueueButton));

    public static readonly StyledProperty<bool> AllowAddToPlaylistActionProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(AllowAddToPlaylistAction), true);

    public static readonly StyledProperty<IEnumerable?> HeroDropdownItemsProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, IEnumerable?>(nameof(HeroDropdownItems));

    public static readonly StyledProperty<object?> HeroDropdownSelectedItemProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, object?>(nameof(HeroDropdownSelectedItem));

    public static readonly StyledProperty<string?> HeroDropdownLabelProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string?>(nameof(HeroDropdownLabel));

    public static readonly StyledProperty<bool> HasHeroDropdownProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(HasHeroDropdown));

    public static readonly StyledProperty<bool> HasHeroDropdownLabelProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(HasHeroDropdownLabel));

    public static readonly StyledProperty<bool> IsLoadingMoreProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(IsLoadingMore));

    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(IsLoading));

    public static readonly StyledProperty<string> LoadingTextProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string>(nameof(LoadingText), "正在加载...");

    public static readonly StyledProperty<string> LoadingMoreTextProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string>(nameof(LoadingMoreText), "正在加载歌曲...");

    public static readonly StyledProperty<ICommand?> HeroActionCommandProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, ICommand?>(nameof(HeroActionCommand));

    public static readonly StyledProperty<string?> HeroActionTextProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string?>(nameof(HeroActionText));

    public static readonly StyledProperty<string?> HeroActionSvgPathProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string?>(nameof(HeroActionSvgPath));

    public static readonly StyledProperty<Geometry?> HeroActionIconDataProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, Geometry?>(nameof(HeroActionIconData));

    public static readonly StyledProperty<bool> HeroActionIsVisibleProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(HeroActionIsVisible), true);

    public static readonly StyledProperty<bool> HasHeroActionProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(HasHeroAction));

    public static readonly StyledProperty<bool> HasHeroActionTextProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(HasHeroActionText));

    public static readonly StyledProperty<bool> HasHeroActionSvgPathProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(HasHeroActionSvgPath));

    public static readonly StyledProperty<bool> HasHeroActionIconDataProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(HasHeroActionIconData));

    public static readonly StyledProperty<Thickness> HeaderMarginProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, Thickness>(nameof(HeaderMargin), new Thickness(0));

    public static readonly StyledProperty<Thickness> ListMarginProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, Thickness>(nameof(ListMargin), new Thickness(0, 15, 0, 0));

    public static readonly StyledProperty<Thickness> ListPaddingProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, Thickness>(nameof(ListPadding), new Thickness(0, 0, 0, 80));

    public static readonly StyledProperty<double> TitleFontSizeProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, double>(nameof(TitleFontSize), 28);

    public static readonly StyledProperty<double> TitleMaxWidthProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, double>(nameof(TitleMaxWidth), 560);

    public static readonly StyledProperty<bool> EnableMultiSelectProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(EnableMultiSelect));

    public static readonly StyledProperty<ICommand?> BatchRemoveCommandProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, ICommand?>(nameof(BatchRemoveCommand));

    public static readonly StyledProperty<ICommand?> BatchCopyToPlaylistCommandProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, ICommand?>(nameof(BatchCopyToPlaylistCommand));

    public static readonly StyledProperty<ICommand?> BatchMoveToPlaylistCommandProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, ICommand?>(nameof(BatchMoveToPlaylistCommand));

    public static readonly StyledProperty<bool> EnableDragReorderProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(EnableDragReorder));

    public static readonly StyledProperty<ICommand?> SongDragReorderCommandProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, ICommand?>(nameof(SongDragReorderCommand));

    /// <summary>「批量操作」按钮进入/退出多选模式（而非弹出勾选对话框）。</summary>
    public static readonly StyledProperty<bool> UseBatchModeButtonProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(UseBatchModeButton));

    /// <summary>当前是否处于批量选择模式。</summary>
    public static readonly StyledProperty<bool> IsBatchSelectionModeProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(IsBatchSelectionMode));

    /// <summary>是否处于批量选择模式（供 XAML 绑定）。</summary>
    public bool IsBatchSelectionMode
    {
        get => GetValue(IsBatchSelectionModeProperty);
        private set => SetValue(IsBatchSelectionModeProperty, value);
    }

    /// <summary>「批量操作」按钮行为：进入多选模式（true）或弹出勾选对话框（false）。</summary>
    public bool UseBatchModeButton
    {
        get => GetValue(UseBatchModeButtonProperty);
        set => SetValue(UseBatchModeButtonProperty, value);
    }

    public SongCollectionDetailView()
    {
        PlayFirstSongCommand = new RelayCommand(PlayFirstSong);
        OpenBatchActionsCommand = new RelayCommand(OpenBatchActions);
        ScrollToPlayingSongCommand = new RelayCommand(ScrollToPlayingSong);
        PlaySongCommand = new AsyncRelayCommand<SongItem?>(PlaySongAsync);
        AddToNextCommand = new RelayCommand<SongItem?>(AddToNext);
        ShowPlaylistDialogCommand = new AsyncRelayCommand<SongItem?>(ShowPlaylistDialogAsync);
        MatchLocalLyricsCommand = new AsyncRelayCommand<SongItem?>(MatchLocalLyricsAsync);
        ViewSingerCommand = new RelayCommand<SingerLite?>(ViewSinger);
        SelectAllSongsCommand = new RelayCommand(SelectAllSongs);
        ClearSongSelectionCommand = new RelayCommand(ClearSongSelection);
        ToggleBatchModeCommand = new RelayCommand(ToggleBatchSelectionMode);
        _songSearchDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(150)
        };
        _songSearchDebounceTimer.Tick += OnSongSearchDebounceTimerTick;
        VisibleSongs = new AvaloniaList<SongItem>();
        InitializeComponent();
        AttachSongListHandlers();
        UpdateCurrentHeroBackground();
    }

    public ICommand PlayFirstSongCommand { get; }
    public ICommand OpenBatchActionsCommand { get; }
    public ICommand ScrollToPlayingSongCommand { get; }
    public ICommand PlaySongCommand { get; }
    public ICommand AddToNextCommand { get; }
    public ICommand ShowPlaylistDialogCommand { get; }
    public ICommand MatchLocalLyricsCommand { get; }
    public ICommand ViewSingerCommand { get; }
    public ICommand SelectAllSongsCommand { get; }
    public ICommand ClearSongSelectionCommand { get; }
    public ICommand ToggleBatchModeCommand { get; }
    public AvaloniaList<SongItem> VisibleSongs { get; }

    public string? Cover
    {
        get => GetValue(CoverProperty);
        set => SetValue(CoverProperty, value);
    }

    public bool ShowCover
    {
        get => GetValue(ShowCoverProperty);
        set => SetValue(ShowCoverProperty, value);
    }

    public bool ShowSongList
    {
        get => GetValue(ShowSongListProperty);
        set => SetValue(ShowSongListProperty, value);
    }

    public bool ShowPlayAllButton
    {
        get => GetValue(ShowPlayAllButtonProperty);
        set => SetValue(ShowPlayAllButtonProperty, value);
    }

    public string HeroBackground
    {
        get => GetValue(HeroBackgroundProperty);
        set => SetValue(HeroBackgroundProperty, value);
    }

    public string LightHeroBackground
    {
        get => GetValue(LightHeroBackgroundProperty);
        set => SetValue(LightHeroBackgroundProperty, value);
    }

    public string NightHeroBackground
    {
        get => GetValue(NightHeroBackgroundProperty);
        set => SetValue(NightHeroBackgroundProperty, value);
    }

    public string CurrentHeroBackground
    {
        get => GetValue(CurrentHeroBackgroundProperty);
        private set => SetValue(CurrentHeroBackgroundProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public bool HasSubtitle
    {
        get => GetValue(HasSubtitleProperty);
        private set => SetValue(HasSubtitleProperty, value);
    }

    public IEnumerable? Songs
    {
        get => GetValue(SongsProperty);
        set => SetValue(SongsProperty, value);
    }

    public bool EnableSongSearch
    {
        get => GetValue(EnableSongSearchProperty);
        set => SetValue(EnableSongSearchProperty, value);
    }

    public string? SongSearchText
    {
        get => GetValue(SongSearchTextProperty);
        set => SetValue(SongSearchTextProperty, value);
    }

    public SongLocateRequest? SongLocateRequest
    {
        get => GetValue(SongLocateRequestProperty);
        set => SetValue(SongLocateRequestProperty, value);
    }

    public ICommand? LoadMoreCommand
    {
        get => GetValue(LoadMoreCommandProperty);
        set => SetValue(LoadMoreCommandProperty, value);
    }

    public ICommand? BackCommand
    {
        get => GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public bool ShowBackButton
    {
        get => GetValue(ShowBackButtonProperty);
        set => SetValue(ShowBackButtonProperty, value);
    }

    public ICommand? PlayFirstCommand
    {
        get => GetValue(PlayFirstCommandProperty);
        set => SetValue(PlayFirstCommandProperty, value);
    }

    public IPlaybackCommands? PlaybackCommands
    {
        get => GetValue(PlaybackCommandsProperty);
        set => SetValue(PlaybackCommandsProperty, value);
    }

    public ISongInteractionService? SongInteractions
    {
        get => GetValue(SongInteractionsProperty);
        set => SetValue(SongInteractionsProperty, value);
    }

    public ICommand? RemoveSongCommand
    {
        get => GetValue(RemoveSongCommandProperty);
        set => SetValue(RemoveSongCommandProperty, value);
    }

    public ICommand? SetLocalSongCoverCommand
    {
        get => GetValue(SetLocalSongCoverCommandProperty);
        set => SetValue(SetLocalSongCoverCommandProperty, value);
    }

    public ICommand? SearchLocalPlaylistsCommand
    {
        get => GetValue(SearchLocalPlaylistsCommandProperty);
        set => SetValue(SearchLocalPlaylistsCommandProperty, value);
    }

    public bool HasPlayFirstCommand
    {
        get => GetValue(HasPlayFirstCommandProperty);
        private set => SetValue(HasPlayFirstCommandProperty, value);
    }

    public bool ShowAddLoadedSongsToQueueButton
    {
        get => GetValue(ShowAddLoadedSongsToQueueButtonProperty);
        set => SetValue(ShowAddLoadedSongsToQueueButtonProperty, value);
    }

    public bool AllowAddToPlaylistAction
    {
        get => GetValue(AllowAddToPlaylistActionProperty);
        set => SetValue(AllowAddToPlaylistActionProperty, value);
    }

    public IEnumerable? HeroDropdownItems
    {
        get => GetValue(HeroDropdownItemsProperty);
        set => SetValue(HeroDropdownItemsProperty, value);
    }

    public object? HeroDropdownSelectedItem
    {
        get => GetValue(HeroDropdownSelectedItemProperty);
        set => SetValue(HeroDropdownSelectedItemProperty, value);
    }

    public string? HeroDropdownLabel
    {
        get => GetValue(HeroDropdownLabelProperty);
        set => SetValue(HeroDropdownLabelProperty, value);
    }

    public bool HasHeroDropdown
    {
        get => GetValue(HasHeroDropdownProperty);
        private set => SetValue(HasHeroDropdownProperty, value);
    }

    public bool HasHeroDropdownLabel
    {
        get => GetValue(HasHeroDropdownLabelProperty);
        private set => SetValue(HasHeroDropdownLabelProperty, value);
    }

    public bool IsLoadingMore
    {
        get => GetValue(IsLoadingMoreProperty);
        set => SetValue(IsLoadingMoreProperty, value);
    }

    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string LoadingText
    {
        get => GetValue(LoadingTextProperty);
        set => SetValue(LoadingTextProperty, value);
    }

    public string LoadingMoreText
    {
        get => GetValue(LoadingMoreTextProperty);
        set => SetValue(LoadingMoreTextProperty, value);
    }

    public ICommand? HeroActionCommand
    {
        get => GetValue(HeroActionCommandProperty);
        set => SetValue(HeroActionCommandProperty, value);
    }

    public string? HeroActionText
    {
        get => GetValue(HeroActionTextProperty);
        set => SetValue(HeroActionTextProperty, value);
    }

    public string? HeroActionSvgPath
    {
        get => GetValue(HeroActionSvgPathProperty);
        set => SetValue(HeroActionSvgPathProperty, value);
    }

    public Geometry? HeroActionIconData
    {
        get => GetValue(HeroActionIconDataProperty);
        set => SetValue(HeroActionIconDataProperty, value);
    }

    public bool HeroActionIsVisible
    {
        get => GetValue(HeroActionIsVisibleProperty);
        set => SetValue(HeroActionIsVisibleProperty, value);
    }

    public bool HasHeroAction
    {
        get => GetValue(HasHeroActionProperty);
        private set => SetValue(HasHeroActionProperty, value);
    }

    public bool HasHeroActionText
    {
        get => GetValue(HasHeroActionTextProperty);
        private set => SetValue(HasHeroActionTextProperty, value);
    }

    public bool HasHeroActionSvgPath
    {
        get => GetValue(HasHeroActionSvgPathProperty);
        private set => SetValue(HasHeroActionSvgPathProperty, value);
    }

    public bool HasHeroActionIconData
    {
        get => GetValue(HasHeroActionIconDataProperty);
        private set => SetValue(HasHeroActionIconDataProperty, value);
    }

    public Thickness HeaderMargin
    {
        get => GetValue(HeaderMarginProperty);
        set => SetValue(HeaderMarginProperty, value);
    }

    public Thickness ListMargin
    {
        get => GetValue(ListMarginProperty);
        set => SetValue(ListMarginProperty, value);
    }

    public Thickness ListPadding
    {
        get => GetValue(ListPaddingProperty);
        set => SetValue(ListPaddingProperty, value);
    }

    public double TitleFontSize
    {
        get => GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    public double TitleMaxWidth
    {
        get => GetValue(TitleMaxWidthProperty);
        set => SetValue(TitleMaxWidthProperty, value);
    }

    /// <summary>启用列表多选（Ctrl/Shift）与批量操作栏。</summary>
    public bool EnableMultiSelect
    {
        get => GetValue(EnableMultiSelectProperty);
        set => SetValue(EnableMultiSelectProperty, value);
    }

    public ICommand? BatchRemoveCommand
    {
        get => GetValue(BatchRemoveCommandProperty);
        set => SetValue(BatchRemoveCommandProperty, value);
    }

    public ICommand? BatchCopyToPlaylistCommand
    {
        get => GetValue(BatchCopyToPlaylistCommandProperty);
        set => SetValue(BatchCopyToPlaylistCommandProperty, value);
    }

    public ICommand? BatchMoveToPlaylistCommand
    {
        get => GetValue(BatchMoveToPlaylistCommandProperty);
        set => SetValue(BatchMoveToPlaylistCommandProperty, value);
    }

    /// <summary>启用歌单内拖拽排序。</summary>
    public bool EnableDragReorder
    {
        get => GetValue(EnableDragReorderProperty);
        set => SetValue(EnableDragReorderProperty, value);
    }

    public ICommand? SongDragReorderCommand
    {
        get => GetValue(SongDragReorderCommandProperty);
        set => SetValue(SongDragReorderCommandProperty, value);
    }

    /// <summary>当前选中的歌曲数量（供批量操作栏显示）。</summary>
    public int SelectedSongCount
    {
        get => GetValue(SelectedSongCountProperty);
        private set => SetValue(SelectedSongCountProperty, value);
    }

    public static readonly StyledProperty<int> SelectedSongCountProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, int>(nameof(SelectedSongCount));

    /// <summary>是否有选中歌曲（控制批量操作栏显隐）。</summary>
    public bool HasSelectedSongs
    {
        get => GetValue(HasSelectedSongsProperty);
        private set => SetValue(HasSelectedSongsProperty, value);
    }

    public static readonly StyledProperty<bool> HasSelectedSongsProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(HasSelectedSongs));

    /// <summary>当前选中的歌曲列表（供批量命令作为 CommandParameter 绑定）。</summary>
    public static readonly StyledProperty<IList?> SelectedSongItemsProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, IList?>(nameof(SelectedSongItems));

    public IList? SelectedSongItems
    {
        get => GetValue(SelectedSongItemsProperty);
        private set => SetValue(SelectedSongItemsProperty, value);
    }

    private void AttachSongListHandlers()
    {
        SongList.SelectionChanged += OnSongListSelectionChanged;
        SongList.AddHandler(PointerPressedEvent, OnSongListPointerPressed, RoutingStrategies.Tunnel);
        SongList.AddHandler(PointerMovedEvent, OnSongListPointerMoved, RoutingStrategies.Tunnel);
        SongList.AddHandler(PointerReleasedEvent, OnSongListPointerReleased, RoutingStrategies.Tunnel);
        SongList.AddHandler(PointerCaptureLostEvent, OnSongListPointerCaptureLost, RoutingStrategies.Tunnel);
    }

    private void OnSongListPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        ResetDragState();
    }

    private void OnSongListSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectedSongCount = SongList.SelectedItems?.Count ?? 0;
        HasSelectedSongs = SelectedSongCount > 0;
        SelectedSongItems = HasSelectedSongs ? SongList.SelectedItems : null;
    }

    private void SelectAllSongs()
    {
        if (EnableMultiSelect)
            SongList.SelectAll();
    }

    private void ClearSongSelection()
    {
        SongList.SelectedItems?.Clear();
        SongList.SelectedIndex = -1;
        SelectedSongCount = 0;
        HasSelectedSongs = false;
        SelectedSongItems = null;
    }

    // ---- 拖拽排序（自绘跟手：封面卡片跟随指针 + 目标行高亮） ----

    private SongItem? _draggingSong;
    private IPointer? _dragPointer;
    private Point _dragStartPoint;
    private Control? _dragHoverItem;

    /// <summary>拖拽卡片是否可见（拖动进行中）。</summary>
    public bool IsDragGhostVisible
    {
        get => GetValue(IsDragGhostVisibleProperty);
        private set => SetValue(IsDragGhostVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> IsDragGhostVisibleProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, bool>(nameof(IsDragGhostVisible));

    public string? DragGhostCover
    {
        get => GetValue(DragGhostCoverProperty);
        private set => SetValue(DragGhostCoverProperty, value);
    }

    public static readonly StyledProperty<string?> DragGhostCoverProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string?>(nameof(DragGhostCover));

    public string? DragGhostTitle
    {
        get => GetValue(DragGhostTitleProperty);
        private set => SetValue(DragGhostTitleProperty, value);
    }

    public static readonly StyledProperty<string?> DragGhostTitleProperty =
        AvaloniaProperty.Register<SongCollectionDetailView, string?>(nameof(DragGhostTitle));

    /// <summary>拖拽卡片的跟手变换。</summary>
    public TranslateTransform DragGhostTransform { get; } = new();

    private void OnSongListPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ResetDragState();
        if (!EnableDragReorder || SongDragReorderCommand is null)
            return;

        var properties = e.GetCurrentPoint(SongList).Properties;
        if (!properties.IsLeftButtonPressed)
            return;

        // 仅允许从行尾的「三横杠」把手上发起拖拽，整行拖拽与点击选择冲突。
        if (FindDragHandle(e.Source) is null)
            return;

        var song = FindSongFromSource(e.Source);
        if (song is null)
            return;

        _draggingSong = song;
        _dragPointer = e.Pointer;
        _dragStartPoint = e.GetPosition(this);
        // 捕获指针：后续移动/释放都路由到 SongList，即使指针移出窗口范围也能跟踪。
        e.Pointer.Capture(SongList);
    }

    private void OnSongListPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_draggingSong is null || !ReferenceEquals(e.Pointer, _dragPointer))
            return;

        var position = e.GetPosition(this);
        if (!IsDragGhostVisible)
        {
            var delta = position - _dragStartPoint;
            if (Math.Abs(delta.X) < 4 && Math.Abs(delta.Y) < 4)
                return;

            // 拖拽起飞：显示跟手卡片。
            DragGhostCover = _draggingSong.Cover;
            DragGhostTitle = _draggingSong.Name;
            IsDragGhostVisible = true;
        }

        DragGhostTransform.X = position.X + 14;
        DragGhostTransform.Y = position.Y + 10;
        UpdateDragHoverHighlight(e);
    }

    private void OnSongListPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var song = _draggingSong;
        var inFlight = IsDragGhostVisible;

        // 先计算落位（优先用拖拽过程中记录的悬停行），再清理拖拽状态。
        var anchor = ResolveDropAnchor(e);
        ResetDragState(e.Pointer);

        if (song is null || !inFlight || SongDragReorderCommand is null)
            return;

        // 落位规则：拖到第 N 行 = 插到该行之后（占据视觉上第 N 个位置，
        // 原第 N 首及以后的歌依次后移）。
        anchor ??= VisibleSongs.AsValueEnumerable().LastOrDefault();
        if (anchor is null || ReferenceEquals(anchor, song))
            return;

        var request = new SongDragReorderRequest(song, anchor, InsertAfter: true);
        if (SongDragReorderCommand.CanExecute(request))
            SongDragReorderCommand.Execute(request);
    }

    /// <summary>
    ///     解析松手位置对应的歌曲：优先用拖拽中的悬停行，其次按几何命中，
    ///     都没有（如拖到列表外）返回 null 由调用方回退到最后一首。
    /// </summary>
    private SongItem? ResolveDropAnchor(PointerEventArgs e)
    {
        if (_dragHoverItem?.DataContext is SongItem hoverSong)
            return hoverSong;

        return FindSongAtPointer(e);
    }

    private void ResetDragState(IPointer? pointer = null)
    {
        if (_dragHoverItem is not null)
        {
            _dragHoverItem.ClearValue(OpacityProperty);
            _dragHoverItem = null;
        }

        _draggingSong = null;
        _dragPointer = null;
        IsDragGhostVisible = false;
        pointer?.Capture(null);
    }

    private void UpdateDragHoverHighlight(PointerEventArgs e)
    {
        var item = FindRowContainerAtPointer(e);
        if (ReferenceEquals(item, _dragHoverItem))
            return;

        if (_dragHoverItem is not null)
            _dragHoverItem.ClearValue(OpacityProperty);

        _dragHoverItem = item;
        if (item is not null)
            item.Opacity = 0.55;
    }

    /// <summary>
    ///     找指针所在的行容器：按 DataContext 是 SongItem 匹配（不依赖具体容器类型），
    ///     Y 带容差判断覆盖；行间空隙时取最近的行。
    /// </summary>
    private Control? FindRowContainerAtPointer(PointerEventArgs e)
    {
        Control? nearest = null;
        var nearestDistance = double.MaxValue;

        foreach (var visual in SongList.GetVisualDescendants())
        {
            if (visual is not Control { DataContext: SongItem } container)
                continue;

            var position = e.GetPosition(container);
            if (position.Y >= -2 && position.Y < container.Bounds.Height + 2)
                return container;

            var distance = position.Y < 0 ? -position.Y : position.Y - container.Bounds.Height;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = container;
            }
        }

        return nearest;
    }

    private SongItem? FindSongAtPointer(PointerEventArgs e)
    {
        return FindRowContainerAtPointer(e)?.DataContext as SongItem;
    }

    private static SongItem? FindSongFromSource(object? source)
    {
        return FindItemContainer(source)?.DataContext as SongItem;
    }

    /// <summary>判断拖拽起点是否落在行尾的排序把手上。</summary>
    private static Control? FindDragHandle(object? source)
    {
        for (Visual? current = source as Visual; current is not null; current = current.GetVisualParent())
        {
            if (current is Control { Classes: { } classes } control && classes.Contains("DragHandle"))
                return control;
        }

        return null;
    }

    private static Control? FindItemContainer(object? source)
    {
        for (Visual? current = source as Visual; current is not null; current = current.GetVisualParent())
        {
            if (current is ListBoxItem { DataContext: SongItem } container)
                return container;
        }

        return null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SubtitleProperty)
            HasSubtitle = !string.IsNullOrWhiteSpace(change.NewValue as string);

        if (change.Property == HeroActionCommandProperty ||
            change.Property == HeroActionTextProperty ||
            change.Property == HeroActionSvgPathProperty ||
            change.Property == HeroActionIconDataProperty ||
            change.Property == HeroActionIsVisibleProperty)
            UpdateHeroActionState();

        if (change.Property == PlayFirstCommandProperty)
            HasPlayFirstCommand = change.NewValue is not null;

        if (change.Property == HeroDropdownItemsProperty ||
            change.Property == HeroDropdownLabelProperty)
            UpdateHeroDropdownState();

        if (change.Property == HeroBackgroundProperty ||
            change.Property == LightHeroBackgroundProperty ||
            change.Property == NightHeroBackgroundProperty)
            UpdateCurrentHeroBackground();

        if (change.Property == SongsProperty)
        {
            DetachSongsCollectionChanged(change.OldValue as IEnumerable);
            AttachSongsCollectionChanged(change.NewValue as IEnumerable);
            RebuildVisibleSongs();
            SyncPlayingState();
            ScheduleLocateRequestedSong();
            ClearSongSelection();
        }

        if (change.Property == EnableMultiSelectProperty)
            UpdateMultiSelectMode();

        if (change.Property == SongSearchTextProperty && EnableSongSearch)
            ScheduleVisibleSongsRebuild();

        if (change.Property == EnableSongSearchProperty)
            RebuildVisibleSongs();

        if (change.Property == SongLocateRequestProperty)
            ScheduleLocateRequestedSong();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SukiTheme.GetInstance().PropertyChanged += OnSukiThemePropertyChanged;
        AttachSongsCollectionChanged(Songs);
        RebuildVisibleSongs();
        AttachPlayerPropertyChanged();
        UpdateCurrentHeroBackground();
        SyncPlayingState();
        ScheduleLocateRequestedSong();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SukiTheme.GetInstance().PropertyChanged -= OnSukiThemePropertyChanged;
        _songSearchDebounceTimer.Stop();
        DetachSongsCollectionChanged(Songs);
        DetachPlayerPropertyChanged();
        base.OnDetachedFromVisualTree(e);
    }

    private void PlayFirstSong()
    {
        if (PlayFirstCommand?.CanExecute(null) == true)
        {
            PlayFirstCommand.Execute(null);
            return;
        }

        var firstSong = VisibleSongs.AsValueEnumerable().FirstOrDefault();
        if (firstSong != null && PlaySongCommand.CanExecute(firstSong))
            PlaySongCommand.Execute(firstSong);
    }

    private void OpenBatchActions()
    {
        // 「批量操作」按钮的两种形态：进入多选模式，或弹出勾选对话框（其它页面沿用旧行为）。
        if (UseBatchModeButton)
        {
            ToggleBatchSelectionMode();
            return;
        }

        var loadedSongs = VisibleSongs.AsValueEnumerable().ToList();
        if (SongInteractions != null)
            _ = SongInteractions.ShowBatchActionsAsync(loadedSongs, AllowAddToPlaylistAction);
    }

    /// <summary>进入/退出批量选择模式。进入后单击歌曲即选中/取消，可批量删除、复制、移动。</summary>
    private void ToggleBatchSelectionMode()
    {
        IsBatchSelectionMode = !IsBatchSelectionMode;
        UpdateMultiSelectMode();
        if (!IsBatchSelectionMode)
            ClearSongSelection();
    }

    private void UpdateMultiSelectMode()
    {
        SongList.SelectionMode = IsBatchSelectionMode
            ? SelectionMode.Multiple | SelectionMode.Toggle
            : SelectionMode.Single;
    }

    private async Task PlaySongAsync(SongItem? song)
    {
        // 批量选择模式下点击歌曲是选中/取消，不触发播放。
        if (song is null || PlaybackCommands is null || IsBatchSelectionMode)
            return;

        var context = VisibleSongs.AsValueEnumerable().ToList();
        await PlaybackCommands.PlayAsync(song, context);
    }

    private void AddToNext(SongItem? song)
    {
        if (song != null)
            PlaybackCommands?.AddToNext(song);
    }

    private async Task ShowPlaylistDialogAsync(SongItem? song)
    {
        if (song != null && SongInteractions != null)
            await SongInteractions.ShowAddToPlaylistDialogAsync(song);
    }

    private async Task MatchLocalLyricsAsync(SongItem? song)
    {
        if (song != null && SongInteractions != null)
            await SongInteractions.MatchLocalLyricsAsync(song);
    }

    private void ViewSinger(SingerLite? singer)
    {
        if (singer != null)
            SongInteractions?.NavigateToSinger(singer);
    }

    private void ScrollToPlayingSong()
    {
        var currentSong = ResolveCurrentPlayingSong();
        var playingSong = currentSong is null
            ? null
            : FindSameSong(VisibleSongs, currentSong);

        if (playingSong is null)
            playingSong = VisibleSongs.AsValueEnumerable().FirstOrDefault(song => song.IsPlaying);

        if (playingSong is null)
            return;

        LocateSong(playingSong, clearSearch: true);
    }

    private SongItem? ResolveCurrentPlayingSong()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return (topLevel?.DataContext as MainWindowViewModel)?.Player.CurrentPlayingSong;
    }

    private void AttachSongsCollectionChanged(IEnumerable? songs)
    {
        if (ReferenceEquals(_songsCollectionNotifier, songs))
            return;

        DetachSongsCollectionChanged(_songsCollectionNotifier as IEnumerable);
        _songsCollectionNotifier = songs as INotifyCollectionChanged;
        if (_songsCollectionNotifier != null)
            _songsCollectionNotifier.CollectionChanged += OnSongsCollectionChanged;
    }

    private void DetachSongsCollectionChanged(IEnumerable? songs)
    {
        var notifier = songs as INotifyCollectionChanged;
        if (notifier != null)
            notifier.CollectionChanged -= OnSongsCollectionChanged;

        if (ReferenceEquals(_songsCollectionNotifier, notifier))
            _songsCollectionNotifier = null;
    }

    private void AttachPlayerPropertyChanged()
    {
        var player = (TopLevel.GetTopLevel(this)?.DataContext as MainWindowViewModel)?.Player;
        if (ReferenceEquals(_subscribedPlayer, player))
            return;

        DetachPlayerPropertyChanged();
        _subscribedPlayer = player;
        if (_subscribedPlayer != null)
            _subscribedPlayer.PropertyChanged += OnPlayerPropertyChanged;
    }

    private void DetachPlayerPropertyChanged()
    {
        if (_subscribedPlayer != null)
            _subscribedPlayer.PropertyChanged -= OnPlayerPropertyChanged;

        _subscribedPlayer = null;
    }

    private void OnSongsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildVisibleSongs();
        SyncPlayingState();
        ScheduleLocateRequestedSong();
    }

    private void ScheduleVisibleSongsRebuild()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(RebuildVisibleSongs);
            return;
        }

        _songSearchDebounceTimer.Stop();
        _songSearchDebounceTimer.Start();
    }

    private void OnSongSearchDebounceTimerTick(object? sender, EventArgs e)
    {
        _songSearchDebounceTimer.Stop();
        RebuildVisibleSongs();
    }

    private void RebuildVisibleSongs()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(RebuildVisibleSongs);
            return;
        }

        var allSongs = Songs?.AsValueEnumerable().OfType<SongItem>().ToList() ?? [];
        var query = SongSearchText?.Trim();
        if (!EnableSongSearch || string.IsNullOrWhiteSpace(query))
        {
            VisibleSongs.Clear();
            VisibleSongs.AddRange(allSongs);
            return;
        }

        var filtered = allSongs
            .AsValueEnumerable()
            .Where(song => MatchesSongSearch(song, query))
            .ToList();

        VisibleSongs.Clear();
        VisibleSongs.AddRange(filtered);
    }

    private static bool MatchesSongSearch(SongItem song, string query)
    {
        return ContainsIgnoreCase(song.Name, query)
               || ContainsIgnoreCase(song.DisplayTitle, query)
               || ContainsIgnoreCase(song.Singer, query)
               || ContainsIgnoreCase(song.AlbumName, query);
    }

    private static bool ContainsIgnoreCase(string? value, string query)
    {
        return !string.IsNullOrWhiteSpace(value)
               && value.Contains(query, StringComparison.CurrentCultureIgnoreCase);
    }

    private void OnPlayerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(PlayerViewModel.CurrentPlayingSong))
            return;

        SyncPlayingState();
    }

    private void SyncPlayingState()
    {
        void Update()
        {
            var currentSong = ResolveCurrentPlayingSong();
            var songs = Songs?.AsValueEnumerable().OfType<SongItem>().ToList();
            if (songs == null || songs.Count == 0)
                return;

            foreach (var song in songs)
                song.IsPlaying = IsSameSong(song, currentSong);
        }

        if (Dispatcher.UIThread.CheckAccess())
            Update();
        else
            Dispatcher.UIThread.Post(Update);
    }

    private void ScheduleLocateRequestedSong()
    {
        Dispatcher.UIThread.Post(TryLocateRequestedSong, DispatcherPriority.Loaded);
    }

    private void TryLocateRequestedSong()
    {
        var request = SongLocateRequest;
        if (request is null || request.Sequence == _lastLocatedRequestSequence)
            return;

        var targetSong = Songs?
            .AsValueEnumerable()
            .OfType<SongItem>()
            .FirstOrDefault(song => song.LocalTrackId == request.LocalTrackId);
        if (targetSong is null)
            return;

        _lastLocatedRequestSequence = request.Sequence;
        LocateSong(targetSong, clearSearch: true);
    }

    private void LocateSong(SongItem targetSong, bool clearSearch)
    {
        var visibleTarget = FindSameSong(VisibleSongs, targetSong);
        if (visibleTarget is null && clearSearch && !string.IsNullOrWhiteSpace(SongSearchText))
        {
            _songSearchDebounceTimer.Stop();
            SongSearchText = "";
            RebuildVisibleSongs();
            visibleTarget = FindSameSong(VisibleSongs, targetSong);
        }

        if (visibleTarget is null)
            return;

        void ScrollAndAdjust()
        {
            SongList.SelectedItem = visibleTarget;
            SongList.ScrollIntoView(visibleTarget);
            AdjustScrollPosition(visibleTarget);
        }

        ScrollAndAdjust();
        Dispatcher.UIThread.Post(ScrollAndAdjust, DispatcherPriority.Loaded);
        Dispatcher.UIThread.Post(ScrollAndAdjust, DispatcherPriority.Background);
    }

    private static SongItem? FindSameSong(IEnumerable<SongItem> songs, SongItem target)
    {
        return songs.AsValueEnumerable().FirstOrDefault(song => IsSameSong(song, target));
    }

    private static bool IsSameSong(SongItem? song, SongItem? target)
    {
        if (song is null || target is null)
            return false;

        if (ReferenceEquals(song, target))
            return true;

        if (song.LocalTrackId > 0 &&
            target.LocalTrackId > 0 &&
            song.LocalTrackId == target.LocalTrackId)
            return true;

        if (!string.IsNullOrWhiteSpace(song.LocalFilePath) &&
            !string.IsNullOrWhiteSpace(target.LocalFilePath) &&
            string.Equals(song.LocalFilePath, target.LocalFilePath, StringComparison.OrdinalIgnoreCase))
            return true;

        return !string.IsNullOrWhiteSpace(song.Hash) &&
               !string.IsNullOrWhiteSpace(target.Hash) &&
               string.Equals(song.Hash, target.Hash, StringComparison.OrdinalIgnoreCase);
    }


    private void AdjustScrollPosition(SongItem playingSong)
    {
        var scrollViewer = SongList.GetVisualDescendants().AsValueEnumerable().OfType<ScrollViewer>().FirstOrDefault();
        if (scrollViewer is null)
            return;

        var songIndex = VisibleSongs.IndexOf(playingSong);
        var target = songIndex >= 0 ? SongList.ContainerFromIndex(songIndex) : null;
        target ??= SongList.GetVisualDescendants()
            .AsValueEnumerable()
            .OfType<Control>()
            .FirstOrDefault(control => ReferenceEquals(control.DataContext, playingSong));
        if (target is null)
            return;

        var targetTopLeft = target.TranslatePoint(new Point(0, 0), scrollViewer);
        if (targetTopLeft is null)
            return;

        var currentOffset = scrollViewer.Offset;
        var desiredY = currentOffset.Y + targetTopLeft.Value.Y - (scrollViewer.Viewport.Height * 0.2);
        var maxOffsetY = Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
        var clampedY = Math.Clamp(desiredY, 0, maxOffsetY);

        scrollViewer.Offset = new Vector(currentOffset.X, clampedY);
    }

    private void UpdateHeroActionState()
    {
        HasHeroActionText = !string.IsNullOrWhiteSpace(HeroActionText);
        HasHeroActionSvgPath = !string.IsNullOrWhiteSpace(HeroActionSvgPath);
        HasHeroActionIconData = HeroActionIconData is not null;
        HasHeroAction = HeroActionIsVisible &&
                        (HeroActionCommand is not null ||
                         HasHeroActionText ||
                         HasHeroActionSvgPath ||
                         HasHeroActionIconData);
    }

    private void UpdateHeroDropdownState()
    {
        HasHeroDropdown = HeroDropdownItems is not null;
        HasHeroDropdownLabel = !string.IsNullOrWhiteSpace(HeroDropdownLabel);
    }

    private void OnSukiThemePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        UpdateCurrentHeroBackground();
    }

    private void UpdateCurrentHeroBackground()
    {
        if (!string.IsNullOrWhiteSpace(HeroBackground))
        {
            CurrentHeroBackground = HeroBackground;
            return;
        }

        CurrentHeroBackground = SukiTheme.GetInstance().ActiveBaseTheme == ThemeVariant.Dark
            ? NightHeroBackground
            : LightHeroBackground;
    }
}
