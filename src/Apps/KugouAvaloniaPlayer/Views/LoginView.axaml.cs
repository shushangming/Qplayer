using System;
using Avalonia;
using Avalonia.Controls;
using KugouAvaloniaPlayer.ViewModels;

namespace KugouAvaloniaPlayer.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // IsQrLoginSelected 初始即为 true，页面首次打开不会触发 OnIsQrLoginSelectedChanged，
        // 因此挂载后主动检查一次；离开页面时二维码会被标记过期，再次进入也会自动刷新。
        if (DataContext is LoginViewModel { IsQrLoginSelected: true } vm)
            _ = vm.RefreshQrCodeAsync();
    }
}