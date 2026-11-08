using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using SukiUI.Controls;

namespace KugouAvaloniaPlayer.Controls;

/// <summary>
///     SukiSideMenu 扩展：SukiSideMenu 是 TreeView(Single|AlwaysSelected)，
///     点击"已选中"的菜单项不会产生任何属性变更，导致 MainWindow 无法感知这次点击。
///     这里在 Tunnel 阶段检测"点击了已选中项"，通过命令通知外部（重新导航回该页根），
///     修复"先进入 transient 详情页后再点侧栏固定页无响应"的二义性问题。
/// </summary>
public class KugouSideMenu : SukiSideMenu
{
    public static readonly StyledProperty<ICommand?> AlreadySelectedItemClickedCommandProperty =
        AvaloniaProperty.Register<KugouSideMenu, ICommand?>(nameof(AlreadySelectedItemClickedCommand));

    /// <summary>点击已选中的菜单项时触发，参数为该项的 DataContext。</summary>
    public ICommand? AlreadySelectedItemClickedCommand
    {
        get => GetValue(AlreadySelectedItemClickedCommandProperty);
        set => SetValue(AlreadySelectedItemClickedCommandProperty, value);
    }

    public KugouSideMenu()
    {
        AddHandler(PointerPressedEvent, OnItemPointerPressed, RoutingStrategies.Tunnel);
    }

    private void OnItemPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var command = AlreadySelectedItemClickedCommand;
        var selected = SelectedItem;
        if (command is null || selected is null)
            return;

        for (Visual? current = e.Source as Visual; current is not null; current = current.GetVisualParent())
        {
            if (current is TreeViewItem { DataContext: { } itemContext } &&
                ReferenceEquals(itemContext, selected))
            {
                if (command.CanExecute(itemContext))
                    command.Execute(itemContext);
                break;
            }
        }
    }
}
