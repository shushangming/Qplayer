using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using KugouAvaloniaPlayer.Models;
using KugouAvaloniaPlayer.ViewModels;

namespace KugouAvaloniaPlayer.Controls;

public partial class SidebarPlaylistItemControl : UserControl
{
    public static readonly StyledProperty<ICommand?> OpenCommandProperty =
        AvaloniaProperty.Register<SidebarPlaylistItemControl, ICommand?>(nameof(OpenCommand));

    public static readonly StyledProperty<ICommand?> DeleteOnlinePlaylistCommandProperty =
        AvaloniaProperty.Register<SidebarPlaylistItemControl, ICommand?>(nameof(DeleteOnlinePlaylistCommand));

    public static readonly StyledProperty<ICommand?> EditLocalPlaylistCommandProperty =
        AvaloniaProperty.Register<SidebarPlaylistItemControl, ICommand?>(nameof(EditLocalPlaylistCommand));

    public static readonly StyledProperty<ICommand?> DeleteLocalPlaylistCommandProperty =
        AvaloniaProperty.Register<SidebarPlaylistItemControl, ICommand?>(nameof(DeleteLocalPlaylistCommand));

    private MenuFlyout? _contextFlyout;
    private TopLevel? _lightDismissTopLevel;
    private bool _contextMenuOpen;

    public SidebarPlaylistItemControl()
    {
        InitializeComponent();

        // 右键菜单双通道触发：既响应 ContextRequested（键盘菜单键 / 系统菜单请求），
        // 也直接监听右键 PointerPressed。列表虚拟化重建、或内部 Button 把事件标记为已处理时，
        // 仍能稳定弹出菜单，避免"有时候右键不显示"。
        AddHandler(PointerPressedEvent, OnRootPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    public ICommand? OpenCommand
    {
        get => GetValue(OpenCommandProperty);
        set => SetValue(OpenCommandProperty, value);
    }

    public ICommand? DeleteOnlinePlaylistCommand
    {
        get => GetValue(DeleteOnlinePlaylistCommandProperty);
        set => SetValue(DeleteOnlinePlaylistCommandProperty, value);
    }

    public ICommand? EditLocalPlaylistCommand
    {
        get => GetValue(EditLocalPlaylistCommandProperty);
        set => SetValue(EditLocalPlaylistCommandProperty, value);
    }

    public ICommand? DeleteLocalPlaylistCommand
    {
        get => GetValue(DeleteLocalPlaylistCommandProperty);
        set => SetValue(DeleteLocalPlaylistCommandProperty, value);
    }

    private void OnRootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            return;

        TryShowContextMenu();
    }

    private void ItemShell_OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (TryShowContextMenu())
            e.Handled = true;
    }

    private bool TryShowContextMenu()
    {
        // 同一次右键会先后触发 PointerPressed 与 ContextRequested，用开关避免重复弹出。
        if (_contextMenuOpen || DataContext is not PlaylistItem item)
            return false;

        var flyout = EnsureContextFlyout();
        PopulateMenu(flyout, item);

        if (flyout.Items.Count == 0)
            return false;

        _contextMenuOpen = true;
        flyout.ShowAt(ItemShell, showAtPointer: true);
        AttachLightDismissHandler();
        return true;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_contextFlyout is not null)
        {
            _contextFlyout.Closed -= OnContextFlyoutClosed;
            _contextFlyout.Hide();
            _contextFlyout.Items.Clear();
            _contextFlyout = null;
        }

        _contextMenuOpen = false;
        DetachLightDismissHandler();
        base.OnDetachedFromVisualTree(e);
    }

    private MenuFlyout EnsureContextFlyout()
    {
        if (_contextFlyout is null)
        {
            _contextFlyout = new MenuFlyout();
            _contextFlyout.Closed += OnContextFlyoutClosed;
        }

        return _contextFlyout;
    }

    private void OnContextFlyoutClosed(object? sender, EventArgs e)
    {
        _contextMenuOpen = false;
        DetachLightDismissHandler();
    }

    private void PopulateMenu(MenuFlyout flyout, PlaylistItem item)
    {
        flyout.Items.Clear();

        switch (item.Type)
        {
            case PlaylistType.Online when item.IsLikePlaylist:
                AddMenuItem(flyout, "移除我喜欢的音乐（本机）", DeleteOnlinePlaylistCommand, item);
                break;
            case PlaylistType.Online:
                // QQ 云端歌单无删除接口，这里只从本机侧栏隐藏，不修改云端数据。
                AddMenuItem(flyout, "从侧栏移除", DeleteOnlinePlaylistCommand, item);
                break;
            case PlaylistType.Album:
                AddMenuItem(flyout, "取消收藏专辑（本机）", DeleteOnlinePlaylistCommand, item);
                break;
            case PlaylistType.Local:
                AddMenuItem(flyout, "重命名", EditLocalPlaylistCommand, item);
                AddMenuItem(flyout, "删除歌单", DeleteLocalPlaylistCommand, item);
                break;
        }
    }

    private static void AddMenuItem(MenuFlyout flyout, string header, ICommand? command, PlaylistItem item)
    {
        if (command == null)
            return;

        flyout.Items.Add(new MenuItem
        {
            Header = header,
            Command = command,
            CommandParameter = item
        });
    }

    private void AttachLightDismissHandler()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null || ReferenceEquals(_lightDismissTopLevel, topLevel))
            return;

        DetachLightDismissHandler();

        _lightDismissTopLevel = topLevel;
        topLevel.AddHandler(
            PointerPressedEvent,
            OnTopLevelPointerPressed,
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
    }

    private void DetachLightDismissHandler()
    {
        _lightDismissTopLevel?.RemoveHandler(
            PointerPressedEvent,
            OnTopLevelPointerPressed);
        _lightDismissTopLevel = null;
    }

    private void OnTopLevelPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _contextFlyout?.Hide();
        DetachLightDismissHandler();
    }
}
